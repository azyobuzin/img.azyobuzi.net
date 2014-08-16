using System;
using System.Globalization;
using ImgAzyobuziV3.Core;
using ImgAzyobuziV3.Core.DataModels;
using Nancy;

namespace ImgAzyobuziV3
{
    public class MainModule : NancyModule
    {
        public static ImgAzyobuziContext context;

        public MainModule()
        {
            Get["/"] = _ =>
            {
                throw new ImgAzyobuziException(Errors.SelectAPI);
            };

            Get["/regex.json"] = _ => Response.AsJson(context.GetRegex());

            Get["/sizes.json"] = _ =>
            {
                var model = context.GetSizes((string)Request.Query.uri);
                context.WriteLog(model, Request.Headers.UserAgent, Request.Headers.Referrer);
                return Response.AsJson(model).WithExpires();
            };

            Get["/redirect.json"] = _ =>
            {
                var model = context.Redirect((string)Request.Query.uri, (string)Request.Query.size);
                context.WriteLog(model, Request.Headers.UserAgent, Request.Headers.Referrer);
                return Response.AsRedirect(model.Location).WithExpires();
            };

            OnError += (ctx, ex) =>
            {
                var model = new ErrorModel(ex);
                return Response.AsJson(model, (HttpStatusCode)Errors.ErrorTable[model.Code].StatusCode);
            };
        }
    }

    internal static class Extensions
    {
        internal static Response WithExpires(this Response response)
        {
            return response.WithHeader("Expires",
                DateTimeOffset.UtcNow.AddDays(10).ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern));
        }
    }
}
