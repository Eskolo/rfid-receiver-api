using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        _logger.LogInformation("Arsch endpoint was called.");
        return Ok("Arsch is here!");
    }
}