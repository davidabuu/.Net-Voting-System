using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _authHelper = new AuthHelper(configuration);
    }
    [HttpPost("RegisterUser")]
    public IActionResult RegisterUsers(UserForRegistration userForRegistration)
    {
        if (userForRegistration.Password == userForRegistration.ConfirmPassword)
        {
            string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingSchema.UserLogin WHERE EmailAddress = '" + userForRegistration.EmailAddress + "'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
            if (existingUsers.Count() == 0)
            {
                UserLogin userLogin = new UserLogin()
                {
                    EmailAddress = userForRegistration.EmailAddress,
                    Password = userForRegistration.Password
                };
                if (_authHelper.SetPassword(userLogin))
                {
                    string sqlCommand = @"EXEC VotingSchema.spRegisterUser
                    @FirstName = @FirstNameParam,
                    @LastName = @LastNameParam,
                    @EmailAddress = @EmailAddressParam";
                    DynamicParameters sqlParameters = new DynamicParameters();
                    sqlParameters.Add("@FirstNameParam", userForRegistration.FirstName);
                    sqlParameters.Add("@LastNameParam", userForRegistration.LastName);
                    sqlParameters.Add("@EmailAddressParam", userForRegistration.EmailAddress);
                    if (_dapper.ExecuteSqlWithParameters(sqlCommand, sqlParameters))
                    {
                        return Ok();
                    }
                    throw new Exception("Failed To Add User");
                }
            }
            throw new Exception("User Already Exists");
        }
        throw new Exception("Password Do not Match");
    }
    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword(UserLogin resetPassword)
    {
        string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingSchema.UserLogin WHERE EmailAddress = '" + resetPassword.EmailAddress + "'";
        IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
        if (existingUsers.Count() != 0)
        {
            if (_authHelper.SetPassword(resetPassword))
            {
                return Ok();
            }

        }
        return StatusCode(404, "User Do not exists");
    }
    [HttpPost("Login")]
    public IActionResult Login(UserLogin userLogin)
    {
        string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingSchema.UserLogin WHERE EmailAddress = '" + userLogin.EmailAddress + "'";
        string sqlCommand = @"EXEC VotingSchema.spLoginConfirmation
        @EmailAddress = @EmailAddressParam";
        DynamicParameters sqlParameter = new DynamicParameters();
        sqlParameter.Add("@EmailAddressParam", userLogin.EmailAddress, DbType.String);
        IEnumerable<string> existingUsers = _dapper.LoadData<string>
        (sqlCheckUserExists);
        Console.WriteLine(sqlCommand);
        if (existingUsers.Count() != 0)
        {
            UserForLoginConfirmation userForConfirmation = _dapper
                           .LoadDataSingleWithParameters<UserForLoginConfirmation>(sqlCommand, sqlParameter);
            Console.WriteLine(userForConfirmation.PasswordHash);
            byte[] passwordHash = _authHelper.GetPasswordHash(userLogin.Password, userForConfirmation.PasswordSalt);
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password!");
                }
                string userIdSql = @"
                SELECT UserId FROM VotingSchema.UserLogin WHERE EmailAddress = '" +
              userLogin.EmailAddress + "'";

                int userId = _dapper.LoadSingleData<int>(userIdSql);

                return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
            }

        }
        return StatusCode(404, "User Do Not Exists");
    }
}