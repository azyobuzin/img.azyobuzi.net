using System.IO;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace ImgAzyobuziNet.Core
{
    public static class ParseUtils
    {
        public static string GetOgImage(IParentNode node)
        {
            return node.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content");
        }
    }
}
