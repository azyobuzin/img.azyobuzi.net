using System;

namespace ImgAzyobuziNet.Core
{
    public interface IPatternProvider
    {
        string ServiceId { get; }
        string ServiceName { get; }
        string Pattern { get; }
        // Type ResolverType { get; }
        IResolver GetResolver(IServiceProvider serviceProvider);
    }
}
