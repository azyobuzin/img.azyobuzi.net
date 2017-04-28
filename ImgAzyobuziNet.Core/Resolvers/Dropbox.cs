// TODO: HTML がまったくの別物になってる
/*
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DropboxProvider : PatternProviderBase<DropboxResolver>
    {
        public override string ServiceId => "Dropbox";

        public override string ServiceName => "Dropbox";

        public override string Pattern => @"^https?://(?:www\.|dl\.)?dropbox\.com/(?:"
            + @"(s/\w+/[^/\?]+|sc/\w+/[\w\-]+/\d+)" // file or one of the album
            + @"|(sc/\w+/[\w\-]+)" // album
            + @")/?(?:[\?#].*)?$";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexFileTest()
        {
            var match = this.GetRegex().Match("https://www.dropbox.com/s/981kxfr3w1ij8a5/%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%202015-12-12%2015.30.03.png?dl=0");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("s/981kxfr3w1ij8a5/%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%202015-12-12%2015.30.03.png");
            Assert.True(() => !match.Groups[2].Success);
        }

        [TestMethod(TestCategory.Static)]
        private void RegexAlbumItemTest()
        {
            var match = this.GetRegex().Match("https://www.dropbox.com/sc/9k6de4c89fqxsf6/AADhRTlg6I_BisJkWqQev2UDa/0/");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("sc/9k6de4c89fqxsf6/AADhRTlg6I_BisJkWqQev2UDa/0");
            Assert.True(() => !match.Groups[2].Success);
        }

        [TestMethod(TestCategory.Static)]
        private void RegexAlbumTest()
        {
            var match = this.GetRegex().Match("https://www.dropbox.com/sc/9k6de4c89fqxsf6/AAAm9DDjcDE0-4zuk3Dx2vCSa");
            Assert.True(() => match.Success);
            Assert.True(() => !match.Groups[1].Success);
            match.Groups[2].Value.Is("sc/9k6de4c89fqxsf6/AAAm9DDjcDE0-4zuk3Dx2vCSa");
        }

        #endregion
    }

    public class DropboxResolver : IResolver
    {
        private readonly IMemoryCache _resolverCache;
        private readonly ILogger _logger;

        public DropboxResolver(IMemoryCache resolverCache, ILogger<DropboxResolver> logger)
        {
            this._resolverCache = resolverCache;
            this._logger = logger;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            if (match.Groups[1].Success)
            { // File
                var id = match.Groups[1].Value;
                var result = await this._resolverCache.GetOrSet(
                    "dropbox-" + id,
                    () => this.FetchFile(id)
                ).ConfigureAwait(false);
                var originalFile = "https://dl.dropbox.com/" + id;

                switch (result.Type)
                {
                    case "photo":
                        return new[] { new ImageInfo(originalFile, result.OgImage, result.OgImage) };
                    case "video":
                        return new[] { new ImageInfo(result.OgImage, result.OgImage, result.OgImage, originalFile, originalFile, originalFile) };
                    default:
                        throw new NotPictureException();
                }
            }
            else
            { // Album
                var id = match.Groups[2].Value;
                var result = await this._resolverCache.GetOrSet(
                    "dropbox-" + id,
                    () => this.FetchAlbum(id)
                ).ConfigureAwait(false);

                return result.ConvertAll(x =>
                {
                    var originalFile = "https://dl.dropbox.com" + new Uri(x.Permalink).AbsolutePath;
                    var video = x.IsVideo ? originalFile : null;
                    return new ImageInfo(
                        x.IsVideo ? x.OgImage : originalFile,
                        x.OgImage,
                        x.OgImage,
                        video,
                        video,
                        video
                    );
                });
            }
        }

        private class CacheItem
        {
            public string Type;
            public string OgImage;
        }

        private struct AlbumItem
        {
            public string OgImage;
            public string Permalink;
            public bool IsVideo;
        }

        private async Task<IHtmlDocument> DownloadHtml(string id)
        {
            IHtmlDocument document;
            using (var hc = new HttpClient())
            {
                var requestUri = "https://www.dropbox.com/" + id;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();
                    document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
                }
            }

            // 200 で "検索している情報が見つかりません。" と返してくる
            if (document.GetElementsByClassName("err").Length > 0)
                throw new ImageNotFoundException();

            return document;
        }

        private async Task<CacheItem> FetchFile(string id)
        {
            var document = await this.DownloadHtml(id).ConfigureAwait(false);

            // body の class に preview-photo または preview-video があるならば対応
            return new CacheItem
            {
                Type = document.Body.ClassList
                    .Where(x => x.StartsWith("preview-", StringComparison.Ordinal))
                    .Select(x => x.Substring(8)).SingleOrDefault(),
                OgImage = ResolverUtils.GetOgImage(document)
            };
        }

        private async Task<AlbumItem[]> FetchAlbum(string id)
        {
            // IParentNode.Children は ChildNodes.OfType<IElement>() とほぼ同じ
            var document = await this.DownloadHtml(id).ConfigureAwait(false);
            var ogImages = document.Head.ChildNodes.OfType<IHtmlMetaElement>().Where(x => x.GetAttribute("property") == "og:image");
            var galleryItems = document.GetElementById("gallery-view-media").ChildNodes.OfType<IHtmlListItemElement>();
            return ogImages.Zip(galleryItems,
                (og, li) => new AlbumItem
                {
                    OgImage = og.GetAttribute("content"),
                    Permalink = li.GetElementsByClassName("file-link")[0].GetAttribute("href"),
                    IsVideo = li.GetElementsByClassName("video-overlay").Length > 0
                })
                .ToArray();
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchPhotoTest()
        {
            var result = await this.FetchFile("s/kdumaqovem1xy3n/%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%202015-12-12%2015.30.03.png").ConfigureAwait(false);
            result.Type.Is("photo");
            Assert.True(() => !string.IsNullOrEmpty(result.OgImage));
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchVideoTest()
        {
            var result = await this.FetchFile("s/ml7r2wi5t1c2p5r/2015-05-26%2007.12.02.mp4").ConfigureAwait(false);
            result.Type.Is("video");
            Assert.True(() => !string.IsNullOrEmpty(result.OgImage));
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchAlbumTest()
        {
            var result = await this.FetchAlbum("sc/tg0u686mqyr3uj5/AADlnzwi7nUzJaEOjTlBAVWca").ConfigureAwait(false);

            foreach (var item in result)
            {
                Assert.True(() => !string.IsNullOrEmpty(item.OgImage));
                Assert.True(() => !string.IsNullOrEmpty(item.Permalink));
            }

            Assert.True(() => result[0].IsVideo);
            Assert.True(() => !result[1].IsVideo && !result[2].IsVideo);
        }

        #endregion
    }
}
*/
