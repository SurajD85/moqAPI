using Microsoft.Extensions.Caching.Memory;

namespace Moq.Business
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string key, out T value);
        void Set<T>(string key, T value, TimeSpan expiration);
    }
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);
        }
    }
}
