using ImgAzyobuziNet.Core;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.DependencyInjection;

namespace ImgAzyobuziNet
{
    internal class WebResolveContext : IResolveContext
    {
        public WebResolveContext(HttpContext httpCtx)
        {
            this.MemoryCache = httpCtx.ApplicationServices.GetRequiredService<IMemoryCache>();
        }

        public IMemoryCache MemoryCache { get; }
    }
}
