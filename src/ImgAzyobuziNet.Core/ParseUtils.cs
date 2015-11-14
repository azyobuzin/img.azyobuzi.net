using System.IO;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;

namespace ImgAzyobuziNet.Core
{
    public static class ParseUtils
    {
        public static async Task<string> GetOgImage(Stream stream)
        {
            var document = await new HtmlParser().ParseAsync(stream).ConfigureAwait(false);
            return document.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content");
        }
    }
}
