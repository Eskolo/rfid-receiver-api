using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using rfid_receiver_api.DataTransferObjects;
using rfid_receiver_api.Models;
using Supabase;
using Supabase.Postgrest.Exceptions;

namespace rfid_receiver_api.Services;

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
    private readonly AppDbContext? _db;
    private readonly Client? _supabase;

    public ItemService(ILogger<ItemService> logger, IConfiguration config, AppDbContext? db = null)
    {
        _logger = logger;

        if (db == null)
        {
            var options = new SupabaseOptions { AutoConnectRealtime = true };

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
            _db = db;
        }
    }

    public async Task<Item> CreateAsync(NewItemDto dto)
    {
        if (_db != null)
        {
            var item = new Item
            {
                Id = dto.TagHexId,
                LocationId = dto.LocationId,
                Name = dto.Name,
                IsPresent = dto.IsInside
            };

            _db.Item.Add(item);
            await _db.SaveChangesAsync();
            await _db.Entry(item).Reference(i => i.LocationNavigation).LoadAsync();
            return item;
        }
        else
        {
            var supaItem = new SupaItem
            {
                Id = dto.TagHexId,
                LocationId = dto.LocationId,
                Name = dto.Name,
                IsPresent = dto.IsInside
            };

            try
            {
                var inserted = await _supabase!.From<SupaItem>().Insert(supaItem);
                var model = inserted.Model ?? throw new InvalidOperationException("Failed to insert item.");
                return new Item
                {
                    Id = model.Id,
                    LocationId = model.LocationId,
                    Name = model.Name,
                    IsPresent = model.IsPresent
                };
            }
            catch (PostgrestException ex) when (ex.StatusCode == 23503)
            {
                throw new KeyNotFoundException("Related location not found.");
            }
        }
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        if (_db != null)
        {
            return await _db.Item.Include(i => i.LocationNavigation).ToListAsync();
        }
        else
        {
            var result = await _supabase!.From<SupaItem>().Get();
            return result.Models.Select(m => new Item
            {
                Id = m.Id,
                LocationId = m.LocationId,
                Name = m.Name,
                IsPresent = m.IsPresent
            });
        }
    }

    public async Task<Item> GetByIdAsync(string hexId)
    {
        if (_db != null)
        {
            var item = await _db.Item.Include(i => i.LocationNavigation)
                                     .FirstOrDefaultAsync(i => i.Id == hexId);
            return item ?? throw new KeyNotFoundException("Item not found.");
        }
        else
        {
            var result = await _supabase!.From<SupaItem>()
                                         .Where(i => i.Id == hexId)
                                         .Single();
            var model = result ?? throw new KeyNotFoundException("Item not found.");
            return new Item
            {
                Id = model.Id,
                LocationId = model.LocationId,
                Name = model.Name,
                IsPresent = model.IsPresent
            };
        }
    }

    public async Task<Item> UpdateAsync(string hexId, NewItemDto dto)
    {
        if (_db != null)
        {
            var item = await _db.Item.FindAsync(hexId)
                        ?? throw new KeyNotFoundException("Item not found.");

            item.LocationId = dto.LocationId;
            item.Name = dto.Name;
            item.IsPresent = dto.IsInside;

            await _db.SaveChangesAsync();
            return item;
        }
        else
        {
            var update = new SupaItem
            {
                Id = hexId,
                LocationId = dto.LocationId,
                Name = dto.Name,
                IsPresent = dto.IsInside
            };

            var result = await _supabase!.From<SupaItem>().Upsert(update);
            var model = result.Model ?? throw new InvalidOperationException("Update failed.");
            return new Item
            {
                Id = model.Id,
                LocationId = model.LocationId,
                Name = model.Name,
                IsPresent = model.IsPresent
            };
        }
    }

    public async Task DeleteAsync(string hexId)
    {
        if (_db != null)
        {
            var item = await _db.Item.FindAsync(hexId)
                        ?? throw new KeyNotFoundException("Item not found.");

            _db.Item.Remove(item);
            await _db.SaveChangesAsync();
        }
        else
        {
            await _supabase!.From<SupaItem>()
                            .Where(i => i.Id == hexId)
                            .Delete();
        }
    }
}
