using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class MemoryService(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ILogger<MemoryService> logger) : Application.Interfaces.IMemoryCache
{
    public T? GetAsync<T>(string key)
    {
        var data = memoryCache.Get<T>(key);
        return data;
    }

    public void RemoveAsync(string key)
    {
        throw new NotImplementedException();
    }

    public void SetAsync<T>(string key, T value, int minutes)
    {
        var cachOptions = new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes),
            SlidingExpiration = TimeSpan.FromMinutes(minutes)
        };
        memoryCache.Set(key, value, cachOptions);
    }

}
