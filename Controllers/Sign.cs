using Microsoft.AspNetCore.Mvc;

namespace TestCore9.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Sign(Api.Interface.IClimsApi capi) : ControllerBase
{
    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Get()
    {
        var claims = capi.GetClims(new Dictionary<string, string> { { "test", "test" } }, "airman", "test");
        var token = capi.GetToken(claims, DateTime.Now.AddYears(10));
        return Ok(token);
    }
}