using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rfid_receiver_api.DataTransferObjects;
using rfid_receiver_api.Services;

namespace rfid_receiver_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
    private readonly IItemService _service;
    private readonly ILogger<ItemController> _logger;

    public ItemController(IItemService service, ILogger<ItemController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] NewItemDto dto)
    {
        try
        {
            var item = await _service.CreateAsync(dto);
            var projectedItem = new
            {
                id = item.Id,
                name = item.Name,
                location = item.Location?.Name,
                isPresent = item.IsPresent
            };
            return CreatedAtAction(nameof(GetById), new { hexId = projectedItem.id }, projectedItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create failed");
            return Problem(ex.Message);
        }
    }

    [HttpGet("getallitems")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _service.GetAllAsync();
            var projectedItems = items.Select(item => new
            {
                id = item.Id,
                name = item.Name,
                location = item.Location?.Name,
                isPresent = item.IsPresent
            });
            return Ok(projectedItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve items");
            return Problem("Failed to retrieve items");
        }
    }

    [HttpGet("{hexId}")]
    public async Task<IActionResult> GetById(string hexId)
    {
        try
        {
            var item = await _service.GetByIdAsync(hexId);
            var projectedItem = new
            {
                id = item.Id,
                name = item.Name,
                location = item.Location?.Name,
                isPresent = item.IsPresent
            };
            return Ok(projectedItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve item with id '{tagId}'", hexId);
            return Problem();
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] NewItemDto dto)
    {
        try
        {
            var item = await _service.UpdateAsync(dto.TagHexId, dto);
            return Ok(item);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed for item with tadId '{tagId}'", dto.TagHexId);
            return Problem();
        }
    }

    [HttpDelete("delete/{hexId}")]
    public async Task<IActionResult> Delete(string hexId)
    {
        try
        {
            await _service.DeleteAsync(hexId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed");
            return Problem();
        }
    }
}

