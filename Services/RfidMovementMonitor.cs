namespace rfid_receiver_api.Services;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using rfid_receiver_api.Hubs;
using rfid_receiver_api.Models;

public class RfidMovementMonitor : BackgroundService
{
    private readonly ILogger<RfidMovementMonitor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<RfidHub> _hub;

    private readonly ConcurrentDictionary<string, TagSeenState> _tags = new();
    private readonly TimeSpan _gracePeriod;
    private readonly TimeSpan _cleanupPeriod;
    private readonly TimeSpan _checkInterval;

    public RfidMovementMonitor(IServiceScopeFactory scopeFactory, IHubContext<RfidHub> hub, ILogger<RfidMovementMonitor> logger, IConfiguration cfg)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _logger = logger;

        _gracePeriod = TimeSpan.FromSeconds(cfg.GetValue("Rfid:GraceSeconds", 3));
        _cleanupPeriod = TimeSpan.FromMinutes(cfg.GetValue("Rfid:CleanupMinutes", 5));
        _checkInterval = TimeSpan.FromMilliseconds(cfg.GetValue("Rfid:CheckMs", 1000));
    }

    public void RegisterRead(string tagId, string itemName, int locationId, string locationName, int rssi)
    {
        DateTime now = DateTime.UtcNow;
        _tags.AddOrUpdate(
            tagId,
            addValueFactory: _ => new TagSeenState
            {
                FirstSeen = now,
                LastSeen = now,
                LocationId = locationId,
                IsHandled = false
            },
            updateValueFactory: (tagId, existingState) =>
            {
                existingState.LastSeen = now;
                existingState.LocationId = locationId;
                existingState.IsHandled = false;

                return existingState;
            });

        _hub.Clients.All.SendAsync(RfidHub.NEW_SCAN_RECEIVED,
                tagId,
                itemName,
                locationName,
                rssi);
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await SweepAsync(token);
            await Task.Delay(_checkInterval, token);
        }
    }

    private async Task SweepAsync(CancellationToken token)
    {
        DateTime now = DateTime.UtcNow;

        foreach (KeyValuePair<string, TagSeenState> entry in _tags.ToArray())
        {
            var tagId = entry.Key;
            var state = entry.Value;

            // Has the tag disappeared long enough?
            if (!state.IsHandled && (now - state.LastSeen) > _gracePeriod)
            {
                await TogglePresenceAsync(tagId, state, token);
            }

            // Is the tag stale?  Clean it up.
            if ((now - state.LastSeen) > _cleanupPeriod)
            {
                _tags.TryRemove(tagId, out _);
            }
        }
    }

    private async Task TogglePresenceAsync(string tagId, TagSeenState state, CancellationToken token)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Item? item = await db.Item.FirstOrDefaultAsync(i => i.Id == tagId, token);
            if (item == null)
            {
                _logger.LogWarning("Tag {TagId} not found in DB", tagId);
                return;
            }

            // toggle
            item.IsPresent = !item.IsPresent;
            item.LocationId = state.LocationId;

            await db.SaveChangesAsync(token);

            // mark as handled so we don’t toggle again until a new scan
            _tags[tagId] = state with { IsHandled = true };

            // notify signalR connections
            await _hub.Clients.All.SendAsync(RfidHub.ITEM_STATUS_UPDATE, tagId, item.IsPresent, token);

            _logger.LogInformation("Tag {TagId} toggled to {Status}", tagId, item.IsPresent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling tag {TagId}", tagId);
        }
    }

    private record class TagSeenState
    {
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsHandled { get; set; } = false;
        public int LocationId { get; set; }
    }
}