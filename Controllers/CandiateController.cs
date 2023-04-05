using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CandidateController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    private readonly AuthHelper _authHelper;

    public CandidateController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _authHelper = new AuthHelper(configuration);
    }
    [HttpPost("RegisterCandidate")]
    public IActionResult RegisterCandidate(CandidateModel candidateModel)
    {
        string sqlCheckAdminExists = @"SELECT EmailAddress FROM VotingSchemaApp.AdminLogin WHERE EmailAddress = '" + candidateModel.AdminEmailAddress + "'";
        if (sqlCheckAdminExists.Count() > 0)
        {
            string sqlCommand = @"EXEC spCreateCandidate
        @FirstName = @FirstNameParam
        @LastName = @LastNameParam,
        @ImageData = @ImageDataParam,
        @CandidateRole = @CandidateRoleParam,
        @AdminEmailAddress = @AdminEmailAddressParam";

            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@FirstNameParam", candidateModel.FirstName, DbType.String);
            dynamicParameters.Add("@LastNameParam", candidateModel.LastName, DbType.String);
            dynamicParameters.Add("@ImageDataParam", candidateModel.ImageData, DbType.Binary);
            dynamicParameters.Add("@CandidateRoleParam", candidateModel.CandidateRole, DbType.String);
            dynamicParameters.Add("@AdminEmailAddress", candidateModel.AdminEmailAddress, DbType.Single);
            if (_dapper.ExecuteSqlWithParameters(sqlCommand, dynamicParameters))
            {
                return Ok();
            }
        }
        return StatusCode(401, "Unauthorized");
    }
}