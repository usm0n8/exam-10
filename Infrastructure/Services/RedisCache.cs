using System.Text.Json;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache) : IRedisCache
{
    public async Task SetAsync<T>(string key, T value, int minutes)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes)
        };
        
        var jsonSerializerOption = new JsonSerializerOptions{WriteIndented =   true};
        
        var jsonData = JsonSerializer.Serialize(value, jsonSerializerOption);
        await cache.SetStringAsync(key, jsonData, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonData = await cache.GetStringAsync(key);
        
        return jsonData != null
            ? JsonSerializer.Deserialize<T>(jsonData)
            : default;
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
}