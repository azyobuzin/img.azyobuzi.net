using System;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.Core
{
    public abstract class PatternProviderBase<T> : IPatternProvider
        where T : IResolver
    {
        public abstract string ServiceId { get; }
        public abstract string ServiceName { get; }
        public abstract string Pattern { get; }

        private ObjectFactory _factory = ActivatorUtilities.CreateFactory(typeof(T), Type.EmptyTypes);

        public IResolver GetResolver(IServiceProvider serviceProvider)
        {
            return (IResolver)_factory(serviceProvider, Array.Empty<object>());
        }
    }
}
