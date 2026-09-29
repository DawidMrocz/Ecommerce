using Microsoft.Extensions.Caching.Memory;
using MroczwareFramework.Attributes.DependencyInjection;
using System.Collections;
using System.Reflection;
using System.Text;

namespace MroczwareFramework.Services.CacheService
{
    [DependencyInjection(typeof(ICacheService))]
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void AddMemoryCache(string key, object input, int absoluteExpirationSeconds = 600, int? slidingExpirationSeconds = null)
        {
            MemoryCacheEntryOptions cacheOptions = new();
            cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(absoluteExpirationSeconds);
            if (slidingExpirationSeconds is not null) cacheOptions.SlidingExpiration = TimeSpan.FromSeconds((double)slidingExpirationSeconds);
            _memoryCache.Set(key, input, cacheOptions);
        }
        public void ClearMemoryCacheForKey(string key)
        {
            IEnumerable<string> keys = FindAllKeys();
            foreach (string item in keys.Where(x => x.Contains(key)))
                _memoryCache.Remove(item);
        }
        public void ClearMemoryCacheForUser(int userId)
        {
            IEnumerable<string> keys = FindAllKeys();
            foreach (string item in keys.Where(x => x.Contains($"-CUK_{userId}_ID")))
                _memoryCache.Remove(item);
        }
        public void ClearMemoryCacheForGroup(string group)
        {
            IEnumerable<string> keys = FindAllKeys();
            foreach (string item in keys.Where(x => x.Contains($"CGK_{group}_GROUP")))
                _memoryCache.Remove(item);
        }
        public string BuildKey(string group, int? userId = null, string? unique = null)
        {
            StringBuilder key = new($"CGK_{group}_GROUP");

            if (userId is not null) key.Append($"-CUK_{userId}_ID");

            if (!string.IsNullOrWhiteSpace(unique)) key.Append($"-CPK_{unique}_UNIQ");

            return key.ToString();
        }
        private IEnumerable<string> FindAllKeys()
        {
            PropertyInfo? field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null)
            {
                ICollection? collection = field.GetValue(_memoryCache) as ICollection;

                if (collection is not null)
                    foreach (object? item in collection)
                    {
                        PropertyInfo? methodInfo = item.GetType().GetProperty("Key");
                        if (methodInfo is null) continue;

                        object? val = methodInfo.GetValue(item);
                        if (val is not null) yield return val.ToString()!;
                    }
            }
        }
    }
}
