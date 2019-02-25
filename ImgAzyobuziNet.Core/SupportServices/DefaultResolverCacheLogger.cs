using System;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class DefaultResolverCacheLogger<T> : IResolverCacheLogger<T>
    {
        // TODO: ログの定義はどこかにまとめておかないと、 EventId 重複に気づかなさそう

        private static readonly Action<ILogger, string, bool, Exception> s_getCacheResult =
            LoggerMessage.Define<string, bool>(LogLevel.Information, new EventId(130, "GetCacheResult"), "Get cache for {Key}, Hit? = {Hit}");

        private readonly ILogger _logger;

        public DefaultResolverCacheLogger(ILogger<T> logger)
        {
            this._logger = logger;
        }

        public void LogCacheHit(string key)
        {
            if (this._logger == null) return;
            s_getCacheResult(this._logger, key, true, null);
        }

        public void LogCacheMiss(string key, Exception exception)
        {
            if (this._logger == null) return;
            s_getCacheResult(this._logger, key, false, exception);
        }
    }
}
