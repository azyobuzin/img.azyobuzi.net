using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Core;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.SupportServices.Twitter
{
    internal class TwitterCredentialsManager
    {
        #region Logging

        private static readonly Func<ILogger, IDisposable> s_beginScope = LoggerMessage.DefineScope("GetToken");

        private static readonly Action<ILogger, bool, Exception> s_startGetToken =
            LoggerMessage.Define<bool>(LogLevel.Information, new EventId(110, "StartGetToken"), "Start GetToken (Retry = {Retry})");

        private static readonly Action<ILogger, TimeSpan, Exception> s_endGetToken =
            LoggerMessage.Define<TimeSpan>(LogLevel.Information, new EventId(111, "EndGetToken"), "End GetToken (Elapsed = {Elapsed})");

        private static readonly Action<ILogger, int, string, Exception> s_errorResponse =
            LoggerMessage.Define<int, string>(LogLevel.Error, new EventId(112, "ErrorResponse"), "Status: {StatusCode}\n{ResponseBody}");

        #endregion

        private static readonly TimeSpan _retryInterval = new TimeSpan(TimeSpan.TicksPerMinute);
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        private readonly ILogger _logger;

        public TwitterCredentialsManager(ILogger<TwitterCredentialsManager> logger)
        {
            this._logger = logger;
        }

        public async ValueTask<TokensBase> CreateTwitterClient(string consumerKey, string consumerSecret)
        {
            if (this._cache.TryGetValue(consumerKey, out var cacheItem))
            {
                if (cacheItem.Client != null) return cacheItem.Client;

                if (DateTime.UtcNow - cacheItem.LastChallengedAt < _retryInterval)
                    throw new TwitterAuthenticationWaitingForRetryException();
            }

            TokensBase client;

            using (this._logger != null ? s_beginScope(this._logger) : null)
            {
                Stopwatch stopwatch = null;

                if (this._logger != null)
                {
                    s_startGetToken(this._logger, cacheItem != null, null);
                    stopwatch = Stopwatch.StartNew();
                }

                try
                {
                    client = await OAuth2.GetTokenAsync(consumerKey, consumerSecret).ConfigureAwait(false);
                }
                catch (TwitterException ex)
                {
                    if (this._logger != null)
                    {
                        s_errorResponse(this._logger, (int)ex.Status, ex.Json, ex);
                    }

                    // ネガティブキャッシュ
                    this._cache[consumerKey] = new CacheItem()
                    {
                        LastChallengedAt = DateTime.UtcNow,
                        Client = null,
                    };

                    throw;
                }

                if (this._logger != null)
                {
                    stopwatch.Stop();
                    s_endGetToken(this._logger, stopwatch.Elapsed, null);
                }
            }

            this._cache[consumerKey] = new CacheItem()
            {
                LastChallengedAt = DateTime.UtcNow,
                Client = client,
            };

            return client;
        }

        private class CacheItem
        {
            public DateTime LastChallengedAt;
            public TokensBase Client;
        }
    }

    public class TwitterAuthenticationWaitingForRetryException : ImgAzyobuziNetException
    {
    }
}
