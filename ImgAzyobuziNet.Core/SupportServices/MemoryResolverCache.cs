using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class MemoryResolverCache : IResolverCache
    {
        private static readonly TimeSpan s_defaultSlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay);

        private readonly IMemoryCache _memoryCache;
        private readonly IOptionsMonitor<ResolverCacheOptions> _options;
        private readonly IResolverCacheLogger _logger;

        public MemoryResolverCache(IMemoryCache resolverCache, IOptionsMonitor<ResolverCacheOptions> options, IResolverCacheLogger<MemoryResolverCache> logger)
        {
            this._memoryCache = resolverCache ?? throw new ArgumentNullException(nameof(resolverCache));
            this._options = options;
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
            var expirationSeconds = (this._options?.CurrentValue ?? new ResolverCacheOptions()).ExpirationSeconds;

            using (var entry = this._memoryCache.CreateEntry(key))
            {
                entry.Value = value;
                entry.SlidingExpiration = expirationSeconds.HasValue ? TimeSpan.FromSeconds(expirationSeconds.Value) : (TimeSpan?)null;
            }

            return Task.CompletedTask;
        }

        public Task DeleteExpiredEntries()
        {
            // SlidingExpiration が勝手にやってくれるので、実装する必要なし
            return Task.CompletedTask;
        }
    }
}
