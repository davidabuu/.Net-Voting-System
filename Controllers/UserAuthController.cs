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
            string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingApp.UserRegistration WHERE EmailAddress = '" + userRegister.EmailAddress + "'";
            Console.WriteLine(sqlCheckUserExists);
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
            DynamicParameters sqlParameters = new DynamicParameters();
            if (existingUsers.Count() == 0)
            {
                UserForRegistration newUserRegister = new UserForRegistration()
                {
                    FirstName = userRegister.FirstName,
                    LastName = userRegister.LastName,
                    EmailAddress = userRegister.EmailAddress,
                    Password = userRegister.Password
                };
                if (_authHelper.SetPassword(newUserRegister))
                {
                    return Ok();

                }
                throw new Exception("Failed To Add User");

            }
            throw new Exception("User Already Exists");
        }
        throw new Exception("Password Do not Match");
    }
    [HttpPost("UserLogin")]
    public IActionResult UserLogin(UserLogin adminLogin)
    {
        string sqlCheckAdminExists = @"SELECT EmailAddress FROM VotingApp.UserRegistration  WHERE EmailAddress = '" + adminLogin.EmailAddress + "'";
        string sqlCommand = @"EXEC spUserLoginConfirmation
        @EmailAddress = @EmailAddressParam";
        DynamicParameters sqlParameter = new DynamicParameters();
        sqlParameter.Add("@EmailAddressParam", adminLogin.EmailAddress, DbType.String);
        IEnumerable<string> existingUsers = _dapper.LoadData<string>
        (sqlCheckAdminExists);
        Console.WriteLine(sqlCommand);
        if (existingUsers.Count() != 0)
        {
            AdminForLoginConfirmation adminForConfirmation = _dapper
                           .LoadDataSingleWithParameters<AdminForLoginConfirmation>(sqlCommand, sqlParameter);
            Console.WriteLine(adminForConfirmation.PasswordHash);
            byte[] passwordHash = _authHelper.GetPasswordHash(adminLogin.Password, adminForConfirmation.PasswordSalt);
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != adminForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password!");
                }
                string adminIdSql = @"
                SELECT AdminId FROM VotingApp.AdminRegistration  WHERE EmailAddress = '" + adminLogin.EmailAddress + "'";
                int adminId = _dapper.LoadSingleData<int>(adminIdSql);
                return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(adminId)}
            });

            }
        }
        return StatusCode(404, " User Do Not Exists");
    }
    // [HttpPost("ResetPasswordAdmin")]
    // public IActionResult ResetPasswordAdmin(AdminLogin adminLoginReset)
    // {
    //     if (_authHelper.SetPasswordAdmin(adminLoginReset))
    //     {
    //         return Ok();
    //     }
    //     throw new Exception("Failed to update password!");
    // }
}