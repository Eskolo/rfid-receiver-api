using Microsoft.AspNetCore.Mvc;
using rfid_receiver_api.Models;
using rfid_receiver_api.Middleware;
using rfid_receiver_api.Services;
using rfid_receiver_api.DataTransferObjects;

namespace rfid_receiver_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RfidReadingController : ControllerBase
{
    private readonly IRfidService _rfidService;
    private readonly ILogger<RfidReadingController> _logger;

    public RfidReadingController(IRfidService rfidService, ILogger<RfidReadingController> logger)
    {
        _rfidService = rfidService;
        _logger = logger;
    }

    [HttpPost("addreading")]
    [ApiKey]
    public async Task<IActionResult> AddReading([FromBody] RfidReadingDto reading)
    {
        var (success, errorMessage) = await _rfidService.ProcessReadingAsync(reading);

        if (!success)
        {
            if (errorMessage?.Contains("not found") == true)
                return NotFound(errorMessage);

            return BadRequest(errorMessage);
        }

        return Ok(new { message = "RFID reading added.", reading });
    }
}
