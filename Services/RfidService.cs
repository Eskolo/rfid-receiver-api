namespace rfid_receiver_api.Services;

using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;
using rfid_receiver_api.DataTransferObjects;
using rfid_receiver_api.Models;

public interface IRfidService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reading"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    Task ProcessReadingAsync(RfidReadingDto reading);
}

public class RfidService : IRfidService
{
    private readonly ILogger<RfidService> _logger;
    private readonly AppDbContext _db;
    private readonly RfidMovementMonitor _monitor;

    public RfidService(ILogger<RfidService> logger, RfidMovementMonitor monitor, AppDbContext dbContext)
    {
        _logger = logger;
        _db = dbContext;
        _monitor = monitor;
    }

    
    public async Task ProcessReadingAsync(RfidReadingDto reading)
    {
        if (reading == null || string.IsNullOrEmpty(reading.TagHexId) || reading.LocationId == 0)
            throw new ArgumentException("Invalid RFID reading: TagHexId or LocationId is missing.");

        var scan = new Scan
        {
            ItemId = reading.TagHexId,
            LocationId = reading.LocationId,
            ScannedAt = reading.Timestamp,
            Rssi = reading.SiganlStren
        };

        var entry = _db.Scan.Add(scan);
        await _db.SaveChangesAsync();

        scan = await _db.Scan.Include(s => s.Item)
                             .Include(s => s.Location)
                             .FirstOrDefaultAsync(s => s.Id == entry.Entity.Id);

        if (scan is null)
        {
            // should never end up here 
            _logger.LogCritical("Newly added entry is not retreivable by its id!");
            throw new KeyNotFoundException("Added scan could not be loaded again!");
        }

        _monitor.RegisterRead(
            scan.ItemId,
            scan.Item?.Name ?? "Unknown",
            scan.LocationId,
            scan.Location?.Name ?? "Unknown",
            scan.Rssi);

        _logger.LogInformation("Tag {TagId} read at Location {LocationId} with Signal {Signal}",
            reading.TagHexId, reading.LocationId, reading.SiganlStren);
    }
}
