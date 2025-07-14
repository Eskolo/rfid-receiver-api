namespace rfid_receiver_api.Models;

using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Location[] defaultLocations =
        [
            new Location { Id = 1, Name = "Container 1 (links)" },
            new Location { Id = 2, Name = "Container 2 (mitte)" },
            new Location { Id = 3, Name = "Container 3 (rechts)" }
        ];

        foreach (Location loc in defaultLocations)
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
