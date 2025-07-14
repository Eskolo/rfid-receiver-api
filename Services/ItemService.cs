namespace rfid_receiver_api.Services;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using rfid_receiver_api.DataTransferObjects;
using rfid_receiver_api.Models;

public interface IItemService
{
    Task<Item> CreateAsync(NewItemDto dto);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item> GetByIdAsync(string hexId);
    Task<Item> UpdateAsync(string hexId, NewItemDto dto);
    Task DeleteAsync(string hexId);
}

public class ItemService : IItemService
{
    private readonly ILogger<ItemService> _logger;
    private readonly AppDbContext _db;

    public ItemService(ILogger<ItemService> logger, IConfiguration config, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<Item> CreateAsync(NewItemDto dto)
    {
        Item? item = new()
        {
            Id = dto.TagHexId,
            LocationId = dto.LocationId,
            Name = dto.Name,
            IsPresent = dto.IsInside
        };

        _db.Item.Add(item);
        await _db.SaveChangesAsync();
        item = await _db.Item.Include(i => i.Location)
                             .FirstOrDefaultAsync(i => i.Id == item.Id);

        if (item is null)
        {
            // should never end up here 
            _logger.LogCritical("Newly added item is not retreivable by its id!");
            throw new KeyNotFoundException("Added item could not be loaded again!");
        }

        _logger.LogInformation("Item '{name}' located in '{locName}' with tagId '{tagId}' has been created",
            item!.Name,
            item.Location.Name,
            item.Id);

        return item;
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _db.Item.Include(i => i.Location).ToListAsync();
    }

    public async Task<Item> GetByIdAsync(string hexId)
    {
        Item? item = await _db.Item.Include(i => i.Location)
                                     .FirstOrDefaultAsync(i => i.Id == hexId);

        return item
            ?? throw new KeyNotFoundException($"Item with id '{hexId}' not found.");
    }

    public async Task<Item> UpdateAsync(string hexId, NewItemDto dto)
    {
        Item item = await _db.Item.FindAsync(hexId)
                        ?? throw new KeyNotFoundException($"Item with id '{hexId}' not found.");

        item.LocationId = dto.LocationId;
        item.Name = dto.Name;
        item.IsPresent = dto.IsInside;

        await _db.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(string hexId)
    {
        Item item = await _db.Item.FindAsync(hexId)
                        ?? throw new KeyNotFoundException($"Item with id '{hexId}' not found.");

        _db.Item.Remove(item);
        await _db.SaveChangesAsync();
    }
}
