using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rfid_receiver_api.Middleware;

namespace rfid_receiver_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArschController : ControllerBase
{
    private readonly ILogger<ArschController> _logger;

    public ArschController(ILogger<ArschController> logger)
    {
        _logger = logger;
    }

    [HttpGet("arsch")]
    public IActionResult GetArsch()
    {
        _logger.LogInformation("Arsch endpoint was called.");
        return Ok("Arsch is here!");
    }

    [Authorize]
    [HttpGet("securearsch")]
    public IActionResult SecureGetArsch()
    {
        _logger.LogInformation("Secure Arsch endpoint was called.");
        return Ok("Secure Arsch is here!");
    }

    [HttpGet("apikeygetarsch")]
    [ApiKey]
    public IActionResult ApiKeyGetArsch()
    {
        _logger.LogInformation("Api-Key Arsch endpoint was called.");
        return Ok("Api-Key Arsch is here!");
    }
}