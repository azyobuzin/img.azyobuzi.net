using System;
using System.Collections.Generic;

namespace ImgAzyobuziNet.Core
{
    public class ResolveResult
    {
        public ResolveResult(IResolver resolver, IReadOnlyList<ImageInfo> images)
        {
            this.Resolver = resolver;
            this.Images = images;
        }

        public ResolveResult(IResolver resolver, Exception exception)
        {
            this.Resolver = resolver;
            this.Exception = exception;
        }

        public IResolver Resolver { get; }
        public IReadOnlyList<ImageInfo> Images { get; }
        public Exception Exception { get; }
    }
}
