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
}