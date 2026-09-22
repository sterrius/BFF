using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MyDataController : ControllerBase
{
    [HttpGet(Name = "MyData")]
    public IActionResult Get()
    {
        HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
        return Ok("My Data");
    }
}
