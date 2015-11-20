using System;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core
{
    internal static class ResolverUtils
    {
        internal static string GetOgImage(IParentNode node)
        {
            return node.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content");
        }

        internal static readonly Action<ILogger, string, Exception> RequestingMessage =
            LoggerMessage.Define<string>(LogLevel.Information, 100, "Requesting: {0}");

        internal static readonly Action<ILogger, string, Exception> HttpResponseMessage =
            LoggerMessage.Define<string>(LogLevel.Debug, 101, "{0}");
    }
}
