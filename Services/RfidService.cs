using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using rfid_receiver_api.DataTransferObjects;
using rfid_receiver_api.Hubs;
using rfid_receiver_api.Models;
using Supabase;
using Supabase.Postgrest.Exceptions;

namespace rfid_receiver_api.Services;

public interface IRfidService
{
    Task<(bool Success, string? ErrorMessage)> ProcessReadingAsync(RfidReadingDto reading);
}

public class RfidService : IRfidService
{
    private readonly ILogger<RfidService> _logger;
    private readonly AppDbContext? _db;
    private readonly Client? _supabase;
    private readonly RfidMovementMonitor _monitor;

    public RfidService(ILogger<RfidService> logger, IConfiguration config, RfidMovementMonitor monitor, AppDbContext? dbContext = null)
    {
        _logger = logger;
        _monitor = monitor;

        if (dbContext == null)
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            string? apiKey = config["SupabaseKey"];
            string? apiUrl = config["SupabaseUrl"];
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogCritical("Ipv6 is not available and no supabase api key or url was provided! Cannot run app like this.");
                Environment.Exit(-1);
            }

            _supabase = new Client(apiUrl, apiKey, options);
            _supabase.InitializeAsync().Wait();
        }
        else
        {
            _db = dbContext;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ProcessReadingAsync(RfidReadingDto reading)
    {
        if (reading == null || string.IsNullOrEmpty(reading.TagHexId) || reading.LocationId == 0)
            return (false, "Invalid reading data.");

        if (_db != null)
        {
            try
            {
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

                _monitor.RegisterRead(scan!.ItemId, entry.Entity.Item!.Name, scan.LocationId, entry.Entity.Location!.Name ?? "Unknown", scan.Rssi);
            }
            catch (Exception)
            {
                //_logger.LogError(ex, "Failed to process scan in database");
                _logger.LogError("Failed to process scan in database for tag id '{tagID}'", reading.TagHexId);
                return (false, "Internal server error.");
            }
        }
        else
        {
            try
            {
                // var scan = new SupaScan
                // {
                //     ItemId = reading.TagHexId,
                //     LocationId = reading.LocationId,
                //     ScannedAt = reading.Timestamp,
                //     Rssi = reading.SiganlStren
                // };

                // var added = await _supabase!.From<SupaScan>().Insert(scan);

                // await _hub.Clients.All.SendAsync("ReceiveMessage",
                //     added.Model?.ItemId ?? "[Unknown Item]",
                //     added.Model?.LocationId ?? -1,
                //     reading.SiganlStren);

                // await _hub.Clients.All.SendAsync("ReceiveMessage",
                //     reading.TagHexId ?? "[Unknown Item]",
                //     reading.LocationId,
                //     reading.SiganlStren);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Supabase insert error: {StatusCode}", ex.StatusCode);

                return ex.StatusCode == 23503
                    ? (false, "Foreign key violation — item or location not found.")
                    : (false, "Supabase error.");
            }
        }

        _logger.LogInformation("Tag {TagId} read at Location {LocationId} with Signal {Signal}",
            reading.TagHexId, reading.LocationId, reading.SiganlStren);

        return (true, null);
    }
}
