using DotnetAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController :  ControllerBase{
    private readonly DataContextDapper _dapper;

    public AuthController(IConfiguration configuration){
        _dapper = new DataContextDapper(configuration);
    }
}