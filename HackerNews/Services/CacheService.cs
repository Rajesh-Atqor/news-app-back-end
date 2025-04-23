using HackerNews.Constants;
using HackerNews.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        public CacheService(IMemoryCache cache) 
        {
            _cache = cache;
        }

        public void SetCache(string key, object value)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CommonConstants.CacheExpirationTime)
            });
        }

        public object GetCache(string key)
        {
            if (_cache.TryGetValue(key, out object value))
            {
                return value;
            }
            return null; 
        }
    }
}
