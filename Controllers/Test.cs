using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestCore9.Controllers;

// * https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=minimal-apis

/// <summary>
/// 測試用的Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("這是一個測試用的Controller")]
public class Test : Controller
{
    /// <summary>
    /// Say Hello~
    /// </summary>
    /// <returns></returns>    
    [EndpointDescription("Say Hello~")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
    [HttpGet]
    [AllowAnonymous]

    public string Get()
    {
        return "Hello World!";
    }

    [HttpGet("Echo/{message}")]
    [EndpointDescription("Echo訊息")]        
    [AllowAnonymous]
    public IActionResult Echo([Description("反應訊息")] string message)
    {
        return Ok(message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("Data")]
    [Authorize]
    [EndpointDescription("Bearer認證測試")]
    [ProducesResponseType<Models.Data>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    public IActionResult GetData()
    {
        return Ok(new Models.Data { Name = "airman", Age = 18 });
    }


}