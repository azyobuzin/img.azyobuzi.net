using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using Jil;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using NX;

namespace ImgAzyobuziNet
{
    internal class ApiV2Interoperation
    {
        private static readonly string[] s_serviceNameWhitelist =
        {
            "My365", // 2019/2/28 サービス終了予告
            "MyPix",
            "ニコニコ動画",
            "ニコニコ静画",
            "OneDrive",
            "Ow.ly",
            "PHOTOHITO",
            "フォト蔵",
            "PIAPRO",
            "SkyDrive",
            "TINAMI",
            "Tumblr",
            "つなビィ",
            "TwitCasting",
            "Twitter",
            "Vimeo",
            "Vine",
            "YouTube"
        };

        private readonly Uri _oldApiUri;
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly TelemetryClient _telemetryClient;

        public ApiV2Interoperation(Uri oldApiUri, IImgAzyobuziNetHttpClient httpClient, TelemetryClient telemetryClient)
        {
            this._oldApiUri = oldApiUri;
            this._httpClient = httpClient;
            this._telemetryClient = telemetryClient;
        }

        private Uri CreateRequestUri(string apiName)
        {
            return new Uri(this._oldApiUri, apiName);
        }

        private Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
        {
            // AutoRedirect は常にオフ
            return this._httpClient.SendAsync(request, false);
        }

        public async Task<IReadOnlyList<ApiV2NameRegexPair>> GetRegex()
        {
            string json;
            var req = new HttpRequestMessage(HttpMethod.Get, this.CreateRequestUri("regex.json"));
            using (var res = await this.SendRequest(req).ConfigureAwait(false))
            {
                res.EnsureSuccessStatusCode();
                json = await res.EnsureSuccessStatusCode().Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<IEnumerable<ApiV2NameRegexPair>>(json)
                .Where(x => s_serviceNameWhitelist.Contains(x.Name))
                .ToArray();
        }

        public async Task<(int StatusCode, byte[] Content)> AllSizes(string query)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                this.CreateRequestUri("all_sizes.json" + query)
            );

            using (var res = await this.SendRequest(req).ConfigureAwait(false))
            {
                var content = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                this.CheckResponseException(content);
                return ((int)res.StatusCode, content);
            }
        }

        public async Task<Either<Uri, (int StatusCode, byte[] Content)>> Redirect(string query)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                this.CreateRequestUri("redirect" + query)
            );

            using (var res = await this.SendRequest(req).ConfigureAwait(false))
            {
                switch (res.StatusCode)
                {
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                        return res.Headers.Location.Inl();
                }

                var content = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                this.CheckResponseException(content);
                return ((int)res.StatusCode, content).Inr();
            }
        }

        private void CheckResponseException(byte[] content)
        {
            if (content == null || content.Length == 0) return;
            if (this._telemetryClient == null) return;

            V2ErrorObject error;
            try
            {
                error = JSON.Deserialize<V2ErrorResponse>(
                    Encoding.UTF8.GetString(content))?.error;
            }
            catch (Exception ex) when (ex is DeserializationException || ex is DecoderFallbackException)
            {
                // JSON ではなかったということで
                return;
            }

            if (error?.code == 5000 && !string.IsNullOrEmpty(error.exception))
            {
                // 5000 ならエラーを記録
                var telemetry = new ExceptionTelemetry()
                {
                    Message = error.exception,
                    SeverityLevel = SeverityLevel.Error
                };
                this._telemetryClient.TrackException(telemetry);
            }
        }
    }
}
