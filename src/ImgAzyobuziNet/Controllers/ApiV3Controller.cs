using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.AspNet.Mvc;

namespace ImgAzyobuziNet.Controllers
{
    [Route("api/v3/[action]")]
    public class ApiV3Controller : Controller
    {
        private static readonly IReadOnlyDictionary<int, ErrorDefinition> errors = new Dictionary<int, ErrorDefinition>
        {
            [4001] = new ErrorDefinition(400, "\"uri\" parameter is required."),
            [4002] = new ErrorDefinition(400, "Unsupported URI."),
            [4003] = new ErrorDefinition(400, "\"size\" parameter is invalid."),
            [4043] = new ErrorDefinition(404, "The image is not found."),
            [4044] = new ErrorDefinition(404, "Not a picture."),
            [4045] = new ErrorDefinition(404, "Not a video."),
            [5000] = new ErrorDefinition(500, "Raised unknown exception on server.")
        };

        [NonAction]
        private IActionResult ErrorResponse(int error, string serviceId = null, Exception ex = null)
        {
            var s = errors[error];
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

        [NonAction]
        private IActionResult HandleException(string serviceId, Exception ex)
        {
            return this.ErrorResponse(
                ex is ImageNotFoundException ? 4043
                : ex is NotPictureException ? 4044
                : 5000, serviceId, ex);
        }

        public IActionResult Services()
        {
            return JilJsonResult.Create(ImgAzyobuziNetService.GetResolvers()
                .ConvertAll(x => new { id = x.ServiceId, name = x.ServiceName, pattern = x.Pattern }));
        }

        public async Task<IActionResult> Redirect([FromQuery] string uri, [FromQuery] string size = "full")
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

            var result = await ImgAzyobuziNetService.Resolve(this.HttpContext.RequestServices, uri).ConfigureAwait(false);

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

        public async Task<IActionResult> Resolve([FromQuery] string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return this.ErrorResponse(4001);

            var result = await ImgAzyobuziNetService.Resolve(this.HttpContext.RequestServices, uri).ConfigureAwait(false);

            if (result == null)
                return this.ErrorResponse(4002);

            if (result.Exception != null)
                return this.HandleException(result.PatternProvider.ServiceId, result.Exception);

            return JilJsonResult.Create(new
            {
                service_id = result.PatternProvider.ServiceId,
                service_name = result.PatternProvider.ServiceName,
                images = result.Images.ConvertAll(x => new
                {
                    full = x.Full,
                    large = x.Large,
                    thumb = x.Thumb,
                    video_full = x.VideoFull,
                    video_large = x.VideoLarge,
                    video_mobile = x.VideoMobile
                })
            });
        }
    }
}
