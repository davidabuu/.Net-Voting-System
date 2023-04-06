using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserAuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    private readonly AuthHelper _authHelper;

    public UserAuthController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _authHelper = new AuthHelper(configuration);
    }
    [HttpPost("RegisterUser")]
    public IActionResult RegisterUser(UserForRegistration userRegister)
    {
        if (userRegister.Password == userRegister.ConfirmPassword)
        {
            string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingAppSchema.UserLogin WHERE EmailAddress = '" + userRegister.EmailAddress + "'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
            if (existingUsers.Count() == 0)
            {
                UserLogin userLogin = new UserLogin()
                {
                    EmailAddress = userRegister.EmailAddress,
                    Password = userRegister.Password
                };
                if (_authHelper.SetPassword(userLogin))
                {
                    string sqlCommand = @"EXEC spRegisterAndLoginUser
                    @FirstName = @FirstNameParam,
                    @LastName = @LastNameParam,
                    @EmailAddress = @EmailAddressParam";
                    DynamicParameters sqlParameters = new DynamicParameters();
                    sqlParameters.Add("@FirstNameParam", userRegister.FirstName);
                    sqlParameters.Add("@LastNameParam", userRegister.LastName);
                    sqlParameters.Add("@EmailAddressParam", userRegister.EmailAddress);
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
    [HttpPost("UserLogin")]
    public IActionResult UserLogin(UserLogin userLogin)
    {
        string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingAppSchema.UserLogin WHERE EmailAddress = '" + userLogin.EmailAddress + "'";
        string sqlCommand = @"EXEC VotingAppSchema.spUserLoginConfirmation
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
                SELECT UserId FROM VotingAppSchema.UserLogin WHERE EmailAddress = '" + userLogin.EmailAddress + "'";
                int userId = _dapper.LoadSingleData<int>(userIdSql);
                return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });

            }
        }
        return StatusCode(404, "User Do Not Exists");
    }
     [HttpPost("ResetPasswordUser")]
    public IActionResult ResetPassword(UserLogin userLoginReset)
    {
        if (_authHelper.SetPassword(userLoginReset))
        {
            return Ok();
        }
        throw new Exception("Failed to update password!");
    }
}