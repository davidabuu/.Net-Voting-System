using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminAuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    private readonly AuthHelper _authHelper;

    public AdminAuthController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _authHelper = new AuthHelper(configuration);
    }
    [HttpPost("RegisterAdmin")]
    public IActionResult RegisterAdmin(AdminRegister adminRegister)
    {

        if (adminRegister.Password == adminRegister.ConfirmPassword)
        {
            string sqlCheckUserExists = @"SELECT EmailAddress FROM VotingAppSchema.AdminLogin WHERE EmailAddress = '" + adminRegister.EmailAddress + "'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
            if (existingUsers.Count() == 0)
            {
                AdminLogin adminLogin = new AdminLogin()
                {
                    EmailAddress = adminRegister.EmailAddress,
                    Password = adminRegister.Password
                };
                if (_authHelper.SetPasswordAdmin(adminLogin))
                {
                    return Ok();
                }
            }
            throw new Exception("User Already Exists");
        }
        throw new Exception("Password Do not Match");
    }
    // [HttpPost("AdminLogin")]
    // public IActionResult AdminLogin(AdminLogin adminLogin)
    // {
    //     string sqlCheckAdminExists = @"SELECT EmailAddress FROM VotingAppSchema.AdminLogin WHERE EmailAddress = '" + adminLogin.EmailAddress + "'";
    //     string sqlCommand = @"EXEC VotingAppSchema.spAdminLoginConfirmation
    //     @EmailAddress = @EmailAddressParam";
    //     DynamicParameters sqlParameter = new DynamicParameters();
    //     sqlParameter.Add("@EmailAddressParam", adminLogin.EmailAddress, DbType.String);
    //     IEnumerable<string> existingUsers = _dapper.LoadData<string>
    //     (sqlCheckAdminExists);
    //     Console.WriteLine(sqlCommand);
    //     if (existingUsers.Count() != 0)
    //     {
    //         AdminForLoginConfirmation userForConfirmation = _dapper
    //                        .LoadDataSingleWithParameters<AdminForLoginConfirmation>(sqlCommand, sqlParameter);
    //         Console.WriteLine(userForConfirmation.PasswordHash);
    //         byte[] passwordHash = _authHelper.GetPasswordHash(adminLogin.Password, userForConfirmation.PasswordSalt);
    //         for (int index = 0; index < passwordHash.Length; index++)
    //         {
    //             if (passwordHash[index] != userForConfirmation.PasswordHash[index])
    //             {
    //                 return StatusCode(401, "Incorrect password!");
    //             }
    //             string adminIdSql = @"
    //             SELECT AdminId FROM VotingAppSchema.AdminLogin WHERE EmailAddress = '" + adminLogin.EmailAddress + "'";
    //             int adminId = _dapper.LoadSingleData<int>(adminIdSql);
    //             return Ok(new Dictionary<string, string> {
    //             {"token", _authHelper.CreateToken(adminId)}
    //         });

    //         }
    //     }
    //     return StatusCode(404, "Admin Do Not Exists");
    // }
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