using Microsoft.AspNetCore.Mvc;
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
        try
        {
            await _rfidService.ProcessReadingAsync(reading);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing reading with id {tagId}", reading.TagHexId);
            return Problem();
        }

        return Ok(new { message = "RFID reading added.", reading });
    }
}
