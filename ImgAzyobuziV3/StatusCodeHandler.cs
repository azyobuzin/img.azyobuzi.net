using ImgAzyobuziV3.Core;
using ImgAzyobuziV3.Core.DataModels;
using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;

namespace ImgAzyobuziV3
{
    public class StatusCodeHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.InternalServerError:
                    return true;
            }
            return false;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (context.Response != null && context.Response.Contents != null && !ReferenceEquals(context.Response.Contents, Response.NoBody))
                return;

            var model = new ErrorModel(
                statusCode == HttpStatusCode.NotFound ? Errors.ApiNotFound
                : statusCode == HttpStatusCode.MethodNotAllowed ? Errors.InvalidMethod
                : Errors.UnknownError
            );

            context.Response = new JsonResponse(model, new JsonNetSerializer()).WithStatusCode(statusCode);
        }
    }
}
