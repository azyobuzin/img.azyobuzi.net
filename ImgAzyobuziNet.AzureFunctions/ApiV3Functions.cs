﻿using System;
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
    public static class ApiV3Functions
    {
        // TODO: ほぼコピペなので、 ASP.NET Core と共通化する

        [FunctionName("ServicesV3")]
        public static IActionResult ServicesV3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "v3/services")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContext(req.HttpContext, service =>
            {
                return new JsonResult(service.GetPatternProviders()
                    .Select(x => new { id = x.ServiceId, name = x.ServiceName, pattern = x.Pattern })
                    .ToArray());
            });
        }

        [FunctionName("RedirectV3")]
        public static Task<IActionResult> RedirectV3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "v3/redirect")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContextAsync(req.HttpContext, async service =>
            {
                var uri = req.Query["uri"].FirstOrDefault();
                if (string.IsNullOrEmpty(uri))
                    return ErrorResponse(4001);

                var size = req.Query["size"].FirstOrDefault();
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
                        return ErrorResponse(4003);
                }

                var result = await service.Resolve(uri).ConfigureAwait(false);

                if (result == null)
                    return ErrorResponse(4002);

                if (result.Exception != null)
                    return HandleException(result.PatternProvider.ServiceId, result.Exception);

                if (result.Images.Count == 0)
                    return ErrorResponse(4043, result.PatternProvider.ServiceId);

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
                    return ErrorResponse(isVideo ? 4045 : 4044, result.PatternProvider.ServiceId);

                return new Redirect302Result(location);
            });
        }

        [FunctionName("ResolveV3")]
        public static Task<IActionResult> ResolveV3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = "v3/resolve")] HttpRequest req)
        {
            return FunctionsEnvironment.DoInContextAsync(req.HttpContext, async service =>
            {
                var uri = req.Query["uri"].FirstOrDefault();
                if (string.IsNullOrEmpty(uri))
                    return ErrorResponse(4001);

                var result = await service.Resolve(uri).ConfigureAwait(false);

                if (result == null)
                    return ErrorResponse(4002);

                if (result.Exception != null)
                    return HandleException(result.PatternProvider.ServiceId, result.Exception);

                return new JsonResult(new
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
            });
        }

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

        private static IActionResult ErrorResponse(int error, string serviceId = null, Exception ex = null)
        {
            var s = s_errors[error];
            return new JsonResult(new
            {
                error = new
                {
                    code = error,
                    message = s.Message,
                    service_id = serviceId,
                    exception = ex?.ToString()
                }
            })
            {
                StatusCode = s.StatusCode
            };
        }

        private static IActionResult HandleException(string serviceId, Exception ex)
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

            return ErrorResponse(error, serviceId, ex);
        }
    }
}
