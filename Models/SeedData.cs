using Microsoft.EntityFrameworkCore;
using rfid_receiver_api.Helper;

namespace rfid_receiver_api.Models;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        if (!NetworkHelper.IsIpv6Available)
        {
            return;
        }

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Needs: using Microsoft.EntityFrameworkCore;

        var defaultLocations = new[]
        {
            new Location { Id = 1, Name = "Container 1 (links)" },
            new Location { Id = 2, Name = "Container 2 (mitte)" },
            new Location { Id = 3, Name = "Container 3 (rechts)" }
        };

        foreach (var loc in defaultLocations)
        {
            if (!await db.Location.AnyAsync(l => l.Id == loc.Id))
            {
                db.Location.Add(loc);
            }
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

    }
}
