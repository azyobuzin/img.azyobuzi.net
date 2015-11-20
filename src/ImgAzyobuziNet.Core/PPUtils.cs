using System;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.Core
{
    internal delegate IResolver ResolverFactory(IServiceProvider serviceProvider);

    internal static class PPUtils
    {
        private static readonly object[] empty = new object[0];

        internal static ResolverFactory CreateFactory<T>() where T : IResolver
        {
            var f = ActivatorUtilities.CreateFactory(typeof(T), Type.EmptyTypes);
            return s => f(s, empty) as IResolver;
        }
    }
}
