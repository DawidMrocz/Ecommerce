using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MroczwareFramework.Services.CacheService
{
    internal interface ICacheService
    {
        public void AddMemoryCache(string key, object input, int absoluteExpirationSeconds = 600, int? slidingExpirationSeconds = null);
        public void ClearMemoryCacheForKey(string key);
        public void ClearMemoryCacheForUser(int userId);
        public void ClearMemoryCacheForGroup(string group);
        public string BuildKey(string group, int? userId = null, string? unique = null);
    }
}
