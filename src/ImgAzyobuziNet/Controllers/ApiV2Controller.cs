using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.AspNet.Mvc;

namespace ImgAzyobuziNet.Controllers
{
    [Route("api")]
    public class ApiV2Controller : Controller
    {
        private struct StatusAndMessage
        {
            public int StatusCode;
            public string Message;
        }

        private static StatusAndMessage S(int statusCode, string message)
        {
            return new StatusAndMessage { StatusCode = statusCode, Message = message };
        }

        private static readonly IReadOnlyDictionary<int, StatusAndMessage> errors = new Dictionary<int, StatusAndMessage>
        {
            [4000] = S(400, "Bad request."),
            [4001] = S(400, "\"uri\" parameter is required."),
            [4002] = S(400, "\"uri\" parameter you requested is not supported."),
            [4003] = S(400, "\"size\" parameter is invalid."),
            [4040] = S(404, "Not Found."),
            [4041] = S(404, "Select API."),
            [4042] = S(404, "API you requested is not found."),
            [4043] = S(404, "The picture you requested is not found."),
            [4044] = S(404, "Your request is not a picture."),
            [4045] = S(404, "Your request is not a video."),
            [4050] = S(405, "The method is not allowed."),
            [4051] = S(405, "Call with GET or HEAD method."),
            [5000] = S(500, "Raised unknown exception on server.")
        };

        private JsonResult ErrorResponse(int error, Exception ex = null)
        {
            var s = errors[error];
            var res = this.Json(new
            {
                error = new
                {
                    code = error,
                    message = s.Message,
                    exception = ex?.ToString()
                }
            });
            res.StatusCode = s.StatusCode;
            return res;
        }

        public JsonResult Index()
        {
            return this.ErrorResponse(4041);
        }

        [AcceptVerbs(new[] { "GET", "HEAD" }, Route = "regex.json")]
        public JsonResult Regex()
        {
            return this.Json(
                ImgAzyobuziNetService.GetResolvers()
                .Select(x => new { name = x.ServiceName, regex = x.Pattern }));
        }

        private JsonResult HandleException(Exception ex)
        {
            return ErrorResponse(
                ex is ImageNotFoundException ? 4043
                : ex is IsNotPictureException ? 4044
                : 5000, ex);
        }

        [AcceptVerbs(new[] { "GET", "HEAD" }, Route = "redirect")]
        [AcceptVerbs(new[] { "GET", "HEAD" }, Route = "redirect.json")]
        public async Task<ActionResult> RedirectAction([FromQuery] string uri, [FromQuery] string size = "full")
        {
            if (string.IsNullOrEmpty(uri))
                return this.ErrorResponse(4001);

            switch (size)
            {
                case "full":
                case "large":
                case "thumb":
                case "video":
                    break;
                default:
                    return this.ErrorResponse(4003);
            }

            var result = await ImgAzyobuziNetService.Resolve(new WebResolveContext(this.HttpContext), uri).ConfigureAwait(false);

            if (result == null)
                return this.ErrorResponse(4002);

            if (result.Exception != null)
                return this.HandleException(result.Exception);

            if (result.Images.Count == 0)
                return this.ErrorResponse(4043);

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
                case "video":
                    location = img.Video;
                    if (location == null)
                        return this.ErrorResponse(4045);
                    break;
                default:
                    throw new Exception("unreachable");
            }

            return this.Redirect(location);
        }

        [AcceptVerbs(new[] { "GET", "HEAD" }, Route = "all_sizes.json")]
        public async Task<ActionResult> AllSizes()
        {
            var uri = this.Request.Query["uri"].FirstOrDefault();
            if (string.IsNullOrEmpty(uri))
                return this.ErrorResponse(4001);

            var result = await ImgAzyobuziNetService.Resolve(new WebResolveContext(this.HttpContext), uri).ConfigureAwait(false);

            if (result == null)
                return this.ErrorResponse(4002);

            if (result.Exception != null)
                return this.HandleException(result.Exception);

            if (result.Images.Count == 0)
                return this.ErrorResponse(4043);

            var img = result.Images[0];

            return this.Json(new
            {
                service = result.Resolver.ServiceName,
                full = img.Full,
                full_https = img.Full,
                large = img.Large,
                large_https = img.Large,
                thumb = img.Thumb,
                thumb_https = img.Thumb,
                video = img.Video,
                video_https = img.Video
            });
        }
    }
}
