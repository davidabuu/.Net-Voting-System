using DotnetAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController :  ControllerBase{
    private readonly DataContextDapper _dapper;

    public UserController(IConfiguration configuration){
        _dapper = new DataContextDapper(configuration);
    }
    [HttpGet("Get Date")]
    public DateTime GetDate(){
        string sqlCommand = "SELECT GETDATE()";
        return _dapper.LoadSingleData<DateTime>(sqlCommand);
    }
}