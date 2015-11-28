using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace ImgAzyobuziNet.Middlewares
{
    public class ApiV2Middleware
    {
        public ApiV2Middleware(RequestDelegate next)
        {
            this.next = next;
        }

        private readonly RequestDelegate next;

        public async Task Invoke(HttpContext context)
        {
            PathString path;
            if (!context.Request.Path.StartsWithSegments("/api", out path) || path.StartsWithSegments("/v3"))
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            var impl = new Impl(context);

            if (!context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                impl.ErrorResponse(4050);
                return;
            }

            try
            {
                switch (path.Value)
                {
                    case "/":
                        impl.Index();
                        break;
                    case "/regex.json":
                        impl.Regex();
                        break;
                    case "/redirect":
                    case "/redirect.json":
                        await impl.Redirect().ConfigureAwait(false);
                        break;
                    case "/all_sizes.json":
                        await impl.AllSizes().ConfigureAwait(false);
                        break;
                    default:
                        impl.ErrorResponse(4042);
                        break;
                }
            }
            catch (Exception ex)
            {
                impl.HandleException(ex);
            }
        }

        private class Impl
        {
            public Impl(HttpContext context)
            {
                this.HttpContext = context;
            }

            private readonly HttpContext HttpContext;
            private HttpRequest Request => this.HttpContext.Request;
            private HttpResponse Response => this.HttpContext.Response;

            private static readonly IReadOnlyDictionary<int, ErrorDefinition> errors = new Dictionary<int, ErrorDefinition>
            {
                [4000] = new ErrorDefinition(400, "Bad request."),
                [4001] = new ErrorDefinition(400, "\"uri\" parameter is required."),
                [4002] = new ErrorDefinition(400, "\"uri\" parameter you requested is not supported."),
                [4003] = new ErrorDefinition(400, "\"size\" parameter is invalid."),
                [4040] = new ErrorDefinition(404, "Not Found."),
                [4041] = new ErrorDefinition(404, "Select API."),
                [4042] = new ErrorDefinition(404, "API you requested is not found."),
                [4043] = new ErrorDefinition(404, "The picture you requested is not found."),
                [4044] = new ErrorDefinition(404, "Your request is not a picture."),
                [4045] = new ErrorDefinition(404, "Your request is not a video."),
                [4050] = new ErrorDefinition(405, "The method is not allowed."),
                [4051] = new ErrorDefinition(405, "Call with GET or HEAD method."),
                [5000] = new ErrorDefinition(500, "Raised unknown exception on server.")
            };

            private void Json<T>(T obj)
            {
                this.Response.ContentType = "application/json; charset=utf-8";
                var body = JsonUtils.Serialize(obj);
                this.Response.ContentLength = body.Length;
                this.Response.Body.Write(body, 0, body.Length);
            }

            public void ErrorResponse(int error, Exception ex = null)
            {
                var s = errors[error];
                this.Response.StatusCode = s.StatusCode;
                this.Json(new
                {
                    error = new
                    {
                        code = error,
                        message = s.Message,
                        exception = ex?.ToString()
                    }
                });
            }

            public void HandleException(Exception ex)
            {
                this.ErrorResponse(
                    ex is ImageNotFoundException ? 4043
                    : ex is NotPictureException ? 4044
                    : 5000, ex);
            }

            public void Index()
            {
                this.ErrorResponse(4041);
            }

            public void Regex()
            {
                this.Json(
                    ImgAzyobuziNetService.GetResolvers()
                    .ConvertAll(x => new { name = x.ServiceName, regex = x.Pattern }));
            }

            public async Task Redirect()
            {
                var uri = this.Request.Query["uri"].FirstOrDefault();
                if (string.IsNullOrEmpty(uri))
                {
                    this.ErrorResponse(4001);
                    return;
                }

                var size = this.Request.Query["size"].FirstOrDefault();
                switch (size)
                {
                    case "full":
                    case "large":
                    case "thumb":
                    case "video":
                        break;
                    case "":
                    case null:
                        size = "full";
                        break;
                    default:
                        this.ErrorResponse(4003);
                        return;
                }

                var result = await ImgAzyobuziNetService.Resolve(this.HttpContext.RequestServices, uri).ConfigureAwait(false);

                if (result == null)
                {
                    this.ErrorResponse(4002);
                    return;
                }

                if (result.Exception != null)
                {
                    this.HandleException(result.Exception);
                    return;
                }

                if (result.Images.Count == 0)
                {
                    this.ErrorResponse(4043);
                    return;
                }

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
                        if (string.IsNullOrEmpty(location))
                        {
                            this.ErrorResponse(4045);
                            return;
                        }
                        goto RETURN;
                    default:
                        throw new Exception("unreachable");
                }

                if (string.IsNullOrEmpty(location))
                {
                    this.ErrorResponse(4044);
                    return;
                }

            RETURN:
                this.Response.StatusCode = 302;
                this.Response.GetTypedHeaders().Location = new Uri(location);
            }

            public async Task AllSizes()
            {
                var uri = this.Request.Query["uri"].FirstOrDefault();
                if (string.IsNullOrEmpty(uri))
                {
                    this.ErrorResponse(4001);
                    return;
                }

                var result = await ImgAzyobuziNetService.Resolve(this.HttpContext.RequestServices, uri).ConfigureAwait(false);

                if (result == null)
                {
                    this.ErrorResponse(4002);
                    return;
                }

                if (result.Exception != null)
                {
                    this.HandleException(result.Exception);
                    return;
                }

                if (result.Images.Count == 0)
                {
                    this.ErrorResponse(4043);
                    return;
                }

                var img = result.Images[0];

                this.Json(new
                {
                    service = result.PatternProvider.ServiceName,
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
}
