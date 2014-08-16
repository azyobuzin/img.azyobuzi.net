using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ImgAzyobuziV3.Core.DataModels;

namespace ImgAzyobuziV3.Core
{
    public static class ApiImplements
    {
        private static ConcurrentDictionary<ImgAzyobuziContext, IReadOnlyList<RegexModel>> regexCache = new ConcurrentDictionary<ImgAzyobuziContext, IReadOnlyList<RegexModel>>();
        public static IReadOnlyList<RegexModel> GetRegex(this ImgAzyobuziContext context)
        {
            return regexCache.GetOrAdd(context,
                ctx => ctx.Resolvers.OrderBy(r => r.ServiceId).Select(r => new RegexModel(r)).ToArray());
        }

        public static SizesModel GetSizes(this ImgAzyobuziContext context, string uri)
        {
            foreach (var r in context.Resolvers)
            {
                var match = r.Pattern.Match(uri);
                if (match.Success)
                    return new SizesModel(r, r.GetImages(context, match), r.GetId(match));
            }

            throw new ImgAzyobuziException(Errors.UriNotSupported);
        }

        public static RedirectResult Redirect(this ImgAzyobuziContext context, string uri, SizeType size)
        {
            foreach (var r in context.Resolvers)
            {
                var match = r.Pattern.Match(uri);
                if (match.Success)
                {
                    var images = r.GetImages(context, match);
                    string location;
                    if (size == SizeType.Video)
                    {
                        var v = images.FirstOrDefault(i => i.Video != null);
                        if (v == null) throw new ImgAzyobuziException(Errors.IsNotVideo);
                        location = v.Video;
                    }
                    else
                    {
                        var i = images.First();
                        switch (size)
                        {
                            case SizeType.Full:
                                location = i.Full;
                                break;
                            case SizeType.Large:
                                location = i.Large;
                                break;
                            case SizeType.Thumb:
                                location = i.Thumb;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                    }
                    return new RedirectResult(location, r.ServiceId, r.GetId(match));
                }
            }

            throw new ImgAzyobuziException(Errors.UriNotSupported);
        }

        public static RedirectResult Redirect(this ImgAzyobuziContext context, string uri, string size)
        {
            SizeType type;
            if (string.IsNullOrWhiteSpace(size))
                type = SizeType.Full;
            else if (!Enum.TryParse(size, true, out type))
                throw new ImgAzyobuziException(Errors.InvalidSizeParam);

            return Redirect(context, uri, type);
        }
    }
}
