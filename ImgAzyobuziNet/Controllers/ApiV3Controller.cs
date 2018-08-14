using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ImgAzyobuziNet.Controllers
{
    [Route("api/v3/[action]"), EnableCors(Startup.ApiCorsPolicyName)]
    public class ApiV3Controller : Controller
    {
        private static readonly IReadOnlyDictionary<int, ErrorDefinition> s_errors = new Dictionary<int, ErrorDefinition>
        {
            [4001] = new ErrorDefinition(400, "\"uri\" parameter is required."),
            [4002] = new ErrorDefinition(400, "Unsupported URI."),
            [4003] = new ErrorDefinition(400, "\"size\" parameter is invalid."),
            [4043] = new ErrorDefinition(404, "The image is not found."),
            [4044] = new ErrorDefinition(404, "Not a picture."),
            [4045] = new ErrorDefinition(404, "Not a video."),
            [5000] = new ErrorDefinition(500, "Raised unknown exception on server.")
        };

        private readonly ImgAzyobuziNetService _imgAzyobuziNetService;
        private readonly TelemetryClient _telemetryClient;

        public ApiV3Controller(ImgAzyobuziNetService imgAzyobuziNetService, TelemetryClient telemetryClient)
        {
            this._imgAzyobuziNetService = imgAzyobuziNetService;
            this._telemetryClient = telemetryClient;
        }

        private IActionResult ErrorResponse(int error, string serviceId = null, Exception ex = null)
        {
            var s = s_errors[error];
            return JilJsonResult.CreateWithStatusCode(new
            {
                error = new
                {
                    code = error,
                    message = s.Message,
                    exception = ex?.ToString()
                }
            }, s.StatusCode);
        }

        private IActionResult HandleException(string serviceId, Exception ex)
        {
            int error;
            switch (ex)
            {
                case ImageNotFoundException _:
                    error = 4043;
                    break;
                case NotPictureException _:
                    error = 4044;
                    break;
                default:
                    if (this._telemetryClient != null)
                    {
                        var telemetry = new ExceptionTelemetry(ex)
                        {
                            SeverityLevel = SeverityLevel.Error
                        };
                        this._telemetryClient.TrackException(telemetry);
                    }

                    error = 5000;
                    break;
            }

            return this.ErrorResponse(error, serviceId, ex);
        }

        public IActionResult Services()
        {
            return JilJsonResult.Create(this._imgAzyobuziNetService.GetPatternProviders()
                .Select(x => new { id = x.ServiceId, name = x.ServiceName, pattern = x.Pattern }));
        }

        public async Task<IActionResult> Redirect(string uri, string size = "full")
        {
            if (string.IsNullOrEmpty(uri))
                return this.ErrorResponse(4001);

            bool isVideo;

            switch (size)
            {
                case "full":
                case "large":
                case "thumb":
                    isVideo = false;
                    break;
                case "video_full":
                case "video_large":
                case "video_mobile":
                    isVideo = true;
                    break;
                case "":
                case null:
                    size = "full";
                    isVideo = false;
                    break;
                default:
                    return this.ErrorResponse(4003);
            }

            var result = await this._imgAzyobuziNetService.Resolve(uri).ConfigureAwait(false);

            if (result == null)
                return this.ErrorResponse(4002);

            if (result.Exception != null)
                return this.HandleException(result.PatternProvider.ServiceId, result.Exception);

            if (result.Images.Count == 0)
                return this.ErrorResponse(4043, result.PatternProvider.ServiceId);

            var img = result.Images[0];
            string location;

            switch (size)
            {
                case "full":
                    location = img.Full;
                    break;
                case "large":
                    location = img.Large;
                    break;
                case "thumb":
                    location = img.Thumb;
                    break;
                case "video_full":
                    location = img.VideoFull;
                    break;
                case "video_large":
                    location = img.VideoLarge;
                    break;
                case "video_mobile":
                    location = img.VideoMobile;
                    break;
                default:
                    throw new Exception("unreachable");
            }

            if (string.IsNullOrEmpty(location))
                return this.ErrorResponse(isVideo ? 4045 : 4044, result.PatternProvider.ServiceId);

            return base.Redirect(location);
        }

        public async Task<IActionResult> Resolve(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return this.ErrorResponse(4001);

            var result = await this._imgAzyobuziNetService.Resolve(uri).ConfigureAwait(false);

            if (result == null)
                return this.ErrorResponse(4002);

            if (result.Exception != null)
                return this.HandleException(result.PatternProvider.ServiceId, result.Exception);

            return JilJsonResult.Create(new
            {
                service_id = result.PatternProvider.ServiceId,
                service_name = result.PatternProvider.ServiceName,
                images = result.Images
                    .Select(x => new
                    {
                        full = x.Full,
                        large = x.Large,
                        thumb = x.Thumb,
                        video_full = x.VideoFull,
                        video_large = x.VideoLarge,
                        video_mobile = x.VideoMobile
                    })
                    .ToArray()
            });
        }
    }
}
