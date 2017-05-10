using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;

namespace ImgAzyobuziNet
{
    internal class ApiV2Interoperation
    {
        private readonly InteroperationOptions _options;
        private readonly IHttpClient _httpClient;

        private readonly string[] s_serviceNameWhitelist =
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

        public ApiV2Interoperation(InteroperationOptions options, IHttpClient httpClient)
        {
            this._options = options;
            this._httpClient = httpClient;
        }
    }
}
