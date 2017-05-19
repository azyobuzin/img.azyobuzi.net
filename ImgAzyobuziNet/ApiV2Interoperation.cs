using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using Jil;

namespace ImgAzyobuziNet
{
    internal class ApiV2Interoperation
    {
        private static readonly string[] s_serviceNameWhitelist =
        {
            "携帯百景",
            "飯テロ.in",
            "My365",
            "MyPix",
            "ニコニコ動画",
            "ニコニコ静画",
            "OneDrive",
            "Ow.ly",
            "Path",
            "Pckles",
            "PHOTOHITO",
            "Photomemo",
            "Big Canvas PhotoShare",
            "フォト蔵",
            "PIAPRO",
            "Pikubo",
            "pixiv",
            "Shamoji",
            "SkyDrive",
            "Streamzoo",
            "TINAMI",
            "Tumblr",
            "つなビィ",
            "ついっぷるフォト",
            "TwitCasting",
            "Twitgoo",
            "TwitrPix",
            "Twitter",
            "Ustream.tv",
            "Via.Me",
            "Vimeo",
            "Vine",
            "yfrog",
            "YouTube"
        };

        private readonly Uri _oldApiUri;
        private readonly IHttpClient _httpClient;

        public ApiV2Interoperation(InteroperationOptions options, IHttpClient httpClient)
        {
            this._oldApiUri = new Uri(options.OldApiUri);
            this._httpClient = httpClient;
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

            return JSON.Deserialize<ApiV2NameRegexPair[]>(json);
        }
    }
}
