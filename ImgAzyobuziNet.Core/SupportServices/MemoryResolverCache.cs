using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class MemoryResolverCache : IResolverCache
    {
        private static readonly TimeSpan s_defaultSlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay);

        private readonly IMemoryCache _memoryCache;
        private readonly IResolverCacheLogger _logger;

        public MemoryResolverCache(IMemoryCache resolverCache, IResolverCacheLogger<MemoryResolverCache> logger)
        {
            this._memoryCache = resolverCache ?? throw new ArgumentNullException(nameof(resolverCache));
            this._logger = logger;
        }

        public ValueTask<(bool Exists, T Value)> TryGetValue<T>(string key)
        {
            var exists = this._memoryCache.TryGetValue(key, out T value);

            if (this._logger != null)
            {
                if (exists) this._logger.LogCacheHit(key);
                else this._logger.LogCacheMiss(key, null);
            }

            return new ValueTask<(bool, T)>((exists, value));
        }

        public Task Set(string key, object value)
        {
            using (var entry = this._memoryCache.CreateEntry(key))
            {
                entry.Value = value;
                entry.SlidingExpiration = s_defaultSlidingExpiration;
            }

            return Task.CompletedTask;
        }
    }
}
