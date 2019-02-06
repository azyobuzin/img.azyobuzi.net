using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.SupportServices
{
    [SuppressMessage("Usage", "IAN0001:DoNotCreateHttpClient")]
    public class DefaultHttpClient : IImgAzyobuziNetHttpClient
    {
        private static readonly Func<ILogger, string, IDisposable> s_beginScope =
            LoggerMessage.DefineScope<string>("HttpClient {0}");

        private static readonly Action<ILogger, HttpMethod, string, Exception> s_httpClientRequestMessage =
            LoggerMessage.Define<HttpMethod, string>(LogLevel.Information, new EventId(100, "HttpClientRequest"), "{0} {1}");

        private static readonly Action<ILogger, int, TimeSpan, Exception> s_httpClientResponseMessage =
            LoggerMessage.Define<int, TimeSpan>(LogLevel.Information, new EventId(101, "HttpClientRequest"), "Status: {0}, Elapsed: {1}");

        private static readonly Action<ILogger, string, Exception> s_httpClientResponseContentMessage =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(102, "HttpClientResponseContent"), "{0}");

        private readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private readonly HttpClient _httpClientWithAutoRedirect = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private readonly ILogger _logger;

        public DefaultHttpClient(ILogger<DefaultHttpClient> logger)
        {
            this._logger = logger;

            var userAgent = new ProductInfoHeaderValue("ImgAzyobuziNet", "3.0");
            this._httpClient.DefaultRequestHeaders.UserAgent.Add(userAgent);
            this._httpClientWithAutoRedirect.DefaultRequestHeaders.UserAgent.Add(userAgent);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool allowAutoRedirect = true)
        {
            using (this._logger != null ? s_beginScope(this._logger, request.RequestUri.Host) : null)
            {
                Stopwatch stopwatch = null;

                if (this._logger != null)
                {
                    s_httpClientRequestMessage(this._logger, request.Method, request.RequestUri.AbsoluteUri, null);
                    stopwatch = Stopwatch.StartNew();
                }

                var client = allowAutoRedirect ? this._httpClientWithAutoRedirect : this._httpClient;
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                if (this._logger != null)
                {
                    stopwatch.Stop();
                    s_httpClientResponseMessage(this._logger, (int)response.StatusCode, stopwatch.Elapsed, null);

                    try
                    {
                        s_httpClientResponseContentMessage(this._logger, await response.Content.ReadAsStringAsync().ConfigureAwait(false), null);
                    }
                    catch (Exception ex)
                    {
                        s_httpClientResponseContentMessage(this._logger, "Error while ReadAsStringAsync", ex);
                    }
                }

                return response;
            }
        }
    }
}
