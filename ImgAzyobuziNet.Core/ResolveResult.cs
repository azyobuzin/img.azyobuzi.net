using System;
using System.Collections.Generic;

namespace ImgAzyobuziNet.Core
{
    public class ResolveResult
    {
        public ResolveResult(IPatternProvider patternProvider, IReadOnlyList<ImageInfo> images)
        {
            this.PatternProvider = patternProvider;
            this.Images = images;
        }

        public ResolveResult(IPatternProvider patternProvider, Exception exception)
        {
            this.PatternProvider = patternProvider;
            this.Exception = exception;
        }

        public IPatternProvider PatternProvider { get; }
        public IReadOnlyList<ImageInfo> Images { get; }
        public Exception Exception { get; }
    }
}
