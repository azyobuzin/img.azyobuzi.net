using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core.SupportServices
{
    public class MemoryResolverCache : IResolverCache
    {
        private static readonly TimeSpan s_defaultSlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay);

        private readonly IMemoryCache _memoryCache;

        public MemoryResolverCache(IMemoryCache resolverCache)
        {
            this._memoryCache = resolverCache;
        }

        public ValueTask<(bool Exists, T Value)> TryGetValue<T>(string key)
        {
            return new ValueTask<(bool, T)>(
                (this._memoryCache.TryGetValue(key, out T value), value)
            );
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
