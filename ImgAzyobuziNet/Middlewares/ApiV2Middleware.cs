using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Core.SupportServices;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NX;

namespace ImgAzyobuziNet.Middlewares
{
    public class ApiV2Middleware
    {
        public ApiV2Middleware(RequestDelegate next)
        {
            this._next = next;
        }

        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api", out PathString path)
                || path.StartsWithSegments("/v3"))
            {
                await this._next(context).ConfigureAwait(false);
                return;
            }

            await ApplyCorsPolicy(context).ConfigureAwait(false);

            var impl = new Impl(context);

            var method = context.Request.Method.ToUpperInvariant();
            if (method != "GET" && method != "HEAD")
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
                        await impl.Regex().ConfigureAwait(false);
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

        private static async Task ApplyCorsPolicy(HttpContext context)
        {
            var corsPolicyProvider = context.RequestServices.GetService<ICorsPolicyProvider>();
            var corsService = context.RequestServices.GetService<ICorsService>();
            if (corsPolicyProvider == null || corsService == null) return;

            var policy = await corsPolicyProvider.GetPolicyAsync(context, Startup.ApiCorsPolicyName).ConfigureAwait(false);
            if (policy == null) return;

            corsService.ApplyResult(
                corsService.EvaluatePolicy(context, policy),
                context.Response
            );
        }

        private class Impl
        {
            public Impl(HttpContext context)
            {
                this.Request = context.Request;
                this.Response = context.Response;
                this._imgAzyobuziNetService = context.RequestServices.GetService<ImgAzyobuziNetService>();

                var oldApiUri = context.RequestServices.GetService<IOptions<ImgAzyobuziNetOptions>>()?.Value?.FallbackV2ApiUri;
                if (!string.IsNullOrEmpty(oldApiUri))
                {
                    this._interoperation = new ApiV2Interoperation(new Uri(oldApiUri), context.RequestServices.GetService<IImgAzyobuziNetHttpClient>());
                }
            }

            private readonly HttpRequest Request;
            private readonly HttpResponse Response;
            private readonly ImgAzyobuziNetService _imgAzyobuziNetService;
            private readonly ApiV2Interoperation _interoperation;

            private static readonly IReadOnlyDictionary<int, ErrorDefinition> s_errors = new Dictionary<int, ErrorDefinition>
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

            private const string JsonContentType = "application/json; charset=utf-8";

            private void Json<T>(T obj)
            {
                this.Response.ContentType = JsonContentType;
                var body = JsonUtils.Serialize(obj);
                this.Response.ContentLength = body.Length;
                this.Response.Body.Write(body, 0, body.Length);
            }

            private void RawJson(int statusCode, byte[] content)
            {
                this.Response.StatusCode = statusCode;
                this.Response.ContentType = JsonContentType;
                this.Response.ContentLength = content.Length;
                this.Response.Body.Write(content, 0, content.Length);
            }

            public void ErrorResponse(int error, Exception ex = null)
            {
                var s = s_errors[error];
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

            public async Task Regex()
            {
                var result = this._imgAzyobuziNetService.GetPatternProviders()
                    .Select(x => new ApiV2NameRegexPair(x.ServiceName, x.Pattern));

                if (this._interoperation != null)
                {
                    result = result.Concat(await this._interoperation.GetRegex().ConfigureAwait(false));
                }

                this.Json(result);
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

                var result = await this._imgAzyobuziNetService.Resolve(uri).ConfigureAwait(false);

                if (result == null)
                {
                    if (this._interoperation != null)
                    {
                        await this.RedirectInterop().ConfigureAwait(false);
                    }
                    else
                    {
                        this.ErrorResponse(4002);
                    }

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
                        location = img.VideoFull;
                        if (string.IsNullOrEmpty(location))
                        {
                            this.ErrorResponse(4045);
                            return;
                        }
                        break;
                    default:
                        throw new Exception("unreachable");
                }

                if (string.IsNullOrEmpty(location))
                {
                    this.ErrorResponse(4044);
                    return;
                }

                this.Response.StatusCode = 302;
                this.Response.GetTypedHeaders().Location = new Uri(location);
            }

            private async Task RedirectInterop()
            {
                (await this._interoperation.Redirect(this.Request.QueryString.ToUriComponent()).ConfigureAwait(false))
                    .Match(
                        location =>
                        {
                            this.Response.StatusCode = 302;
                            this.Response.GetTypedHeaders().Location = location;
                        },
                        t => this.RawJson(t.StatusCode, t.Content)
                    );
            }

            public async Task AllSizes()
            {
                var uri = this.Request.Query["uri"].FirstOrDefault();
                if (string.IsNullOrEmpty(uri))
                {
                    this.ErrorResponse(4001);
                    return;
                }

                var result = await this._imgAzyobuziNetService.Resolve(uri).ConfigureAwait(false);

                if (result == null)
                {
                    if (this._interoperation != null)
                    {
                        await this.AllSizesInterop().ConfigureAwait(false);
                    }
                    else
                    {
                        this.ErrorResponse(4002);
                    }

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
                    video = img.VideoFull,
                    video_https = img.VideoFull
                });
            }

            private async Task AllSizesInterop()
            {
                var t = await this._interoperation.AllSizes(this.Request.QueryString.ToUriComponent()).ConfigureAwait(false);
                this.RawJson(t.StatusCode, t.Content);
            }
        }
    }
}
