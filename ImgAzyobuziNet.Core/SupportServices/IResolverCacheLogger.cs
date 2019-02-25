using System;

namespace ImgAzyobuziNet.Core.SupportServices
{
    public interface IResolverCacheLogger
    {
        void LogCacheHit(string key);
        void LogCacheMiss(string key, Exception exception);
    }

    public interface IResolverCacheLogger<T> : IResolverCacheLogger { }
}
