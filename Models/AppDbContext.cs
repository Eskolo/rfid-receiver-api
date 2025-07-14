namespace rfid_receiver_api.Models;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Item> Item { get; set; }

    public DbSet<Location> Location { get; set; }
    
    public DbSet<Scan> Scan { get; set; }
}
