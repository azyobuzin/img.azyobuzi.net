using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ImgAzyobuziNet.AzureFunctions
{
    public static class ApiV2Functions
    {
        // TODO: ほぼコピペなので、 ASP.NET Core と共通化する

        [FunctionName("RegexV2")]
        public static IActionResult RegexV2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "regex.json")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContext(req.HttpContext, service =>
            {
                var result = service.GetPatternProviders()
                    .Select(x => new ApiV2NameRegexPair(x.ServiceName, x.Pattern))
                    .ToArray();
                return new JsonResult(result);
            });
        }

        [FunctionName("RedirectV2")]
        public static Task<IActionResult> RedirectV2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "redirect")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContextAsync(req.HttpContext, async service =>
            {
                try
                {
                    var uri = req.Query["uri"].FirstOrDefault();
                    if (string.IsNullOrEmpty(uri))
                    {
                        return CreateErrorResponse(4001);
                    }

                    var size = req.Query["size"].FirstOrDefault();
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
                            return CreateErrorResponse(4003);
                    }

                    var result = await service.Resolve(uri).ConfigureAwait(false);

                    if (result == null)
                    {
                        return CreateErrorResponse(4002);
                    }

                    if (result.Exception != null)
                    {
                        return CreateErrorResponse(result.Exception);
                    }

                    if (result.Images.Count == 0)
                    {
                        return CreateErrorResponse(4043);
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
                                return CreateErrorResponse(4045);
                            }
                            break;
                        default:
                            throw new Exception("unreachable");
                    }

                    if (string.IsNullOrEmpty(location))
                    {
                        return CreateErrorResponse(4044);
                    }

                    return new Redirect302Result(location);
                }
                catch (Exception ex)
                {
                    return CreateErrorResponse(ex);
                }
            });
        }

        [FunctionName("RedirectV2Json")]
        public static Task<IActionResult> RedirectV2Json(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "redirect.json")] HttpRequest req)
        {
            return RedirectV2(req);
        }

        [FunctionName("AllSizesV2")]
        public static Task<IActionResult> AllSizesV2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "all_sizes.json")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContextAsync(req.HttpContext, async service =>
            {
                try
                {
                    var uri = req.Query["uri"].FirstOrDefault();
                    if (string.IsNullOrEmpty(uri))
                    {
                        return CreateErrorResponse(4001);
                    }

                    var result = await service.Resolve(uri).ConfigureAwait(false);

                    if (result == null)
                    {
                        return CreateErrorResponse(4002);
                    }

                    if (result.Exception != null)
                    {
                        return CreateErrorResponse(result.Exception);
                    }

                    if (result.Images.Count == 0)
                    {
                        return CreateErrorResponse(4043);
                    }

                    var img = result.Images[0];

                    return new JsonResult(new
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
                catch (Exception ex)
                {
                    return CreateErrorResponse(ex);
                }
            });
        }

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

        private static IActionResult CreateErrorResponse(int error, Exception ex = null)
        {
            var s = s_errors[error];
            return new JsonResult(new V2ErrorResponse()
            {
                error = new V2ErrorObject()
                {
                    code = error,
                    message = s.Message,
                    exception = ex?.ToString()
                }
            })
            {
                StatusCode = s.StatusCode
            };
        }

        private static IActionResult CreateErrorResponse(Exception ex)
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
                    var telemetry = new ExceptionTelemetry(ex)
                    {
                        SeverityLevel = SeverityLevel.Error
                    };
                    FunctionsEnvironment.TelemetryClient.TrackException(telemetry);

                    error = 5000;
                    break;
            }

            return CreateErrorResponse(error, ex);
        }
    }
}
