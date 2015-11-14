using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
    internal static class Extensions
    {
        private static readonly MemoryCacheEntryOptions defaultOptions = new MemoryCacheEntryOptions()
        {
            SlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay)
        };

        internal static object SetWithDefaultExpiration(this IMemoryCache m, object key, object value)
        {
            return m.Set(key, value, defaultOptions);
        }
    }
}
