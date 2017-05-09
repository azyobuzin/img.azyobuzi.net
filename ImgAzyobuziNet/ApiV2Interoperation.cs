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

        public ApiV2Interoperation(InteroperationOptions options, IHttpClient httpClient)
        {
            this._options = options;
            this._httpClient = httpClient;
        }
    }
}
