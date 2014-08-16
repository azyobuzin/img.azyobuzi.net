using System;
using System.Collections.Generic;

namespace ImgAzyobuziV3.Core
{
    public struct ErrorInfo
    {
        public ErrorInfo(int statusCode, string message)
        {
            this.statusCode = statusCode;
            this.message = message;
        }

        private int statusCode;
        public int StatusCode { get { return this.statusCode; } }

        private string message;
        public string Message { get { return this.message; } }
    }

    public static class Errors
    {
        public const int RequireUriParam = 4001;
        public const int UriNotSupported = 4002;
        public const int InvalidSizeParam = 4003;
        public const int SelectAPI = 4041;
        public const int ApiNotFound = 4042;
        public const int PictureNotFound = 4043;
        public const int IsNotPicture = 4044;
        public const int IsNotVideo = 4045;
        public const int InvalidMethod = 4051;
        public const int UnknownError = 5000;

        public static readonly IReadOnlyDictionary<int, ErrorInfo> ErrorTable = new Dictionary<int, ErrorInfo>()
        {
            { 4000, new ErrorInfo(400, "Bad request.") },
            { RequireUriParam, new ErrorInfo(400, "\"uri\" parameter is required.") },
            { UriNotSupported, new ErrorInfo(400, "\"uri\" parameter you requested is not supported.") },
            { InvalidSizeParam, new ErrorInfo(400, "\"size\" parameter is invalid.") },
            { 4040, new ErrorInfo(404, "Not Found.") },
            { SelectAPI, new ErrorInfo(404, "Welcome to img.azyobuzi.net!") },
            { ApiNotFound, new ErrorInfo(404, "The API you requested is not found.") },
            { PictureNotFound, new ErrorInfo(404, "The picture you requested is not found.") },
            { IsNotPicture, new ErrorInfo(404, "Your request is not a picture.") },
            { IsNotVideo, new ErrorInfo(404, "Your request is not a video.") },
            { 4050, new ErrorInfo(405, "The method is not allowed.") },
            { InvalidMethod, new ErrorInfo(405, "Call with GET method.") },
            { UnknownError, new ErrorInfo(500, "Raised unknown exception on server.") }
        };
    }

    public class ImgAzyobuziException : Exception
    {
        public ImgAzyobuziException(int errorCode, Exception innerException = null)
            : base(Errors.ErrorTable[errorCode].Message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; private set; }
    }
}
