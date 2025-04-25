using HackerNews.Business.Constants;
using HackerNews.Business.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Business.Services
{
    /// <summary>
    /// Provides caching functionality using in-memory cache.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="cache">An instance of <see cref="IMemoryCache"/> for managing cached data.</param>
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Stores an object in the cache with an expiration time.
        /// </summary>
        /// <param name="key">The key used to store and retrieve the cached value.</param>
        /// <param name="value">The object to cache.</param>
        public void SetCache(string key, object value)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CommonConstants.CacheExpirationTime)
            });
        }

        /// <summary>
        /// Retrieves a cached object by key.
        /// </summary>
        /// <param name="key">The key of the cached item to retrieve.</param>
        /// <returns>The cached object if found; otherwise, <c>null</c>.</returns>
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

