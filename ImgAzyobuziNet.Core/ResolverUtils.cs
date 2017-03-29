using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;

namespace ImgAzyobuziNet.Core
{
    internal static class ResolverUtils
    {
        internal static string GetOgImage(INode node)
        {
            return node.Descendents<IHtmlMetaElement>()
                .First(x => x.GetAttribute("property") == "og:image")
                .GetAttribute("content");
        }
    }
}
