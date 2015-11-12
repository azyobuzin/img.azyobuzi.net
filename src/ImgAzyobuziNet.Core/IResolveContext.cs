using Microsoft.Framework.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
	public interface IResolveContext
	{
		IMemoryCache MemoryCache { get; }
	}
	
	public class DefaultResolveContext : IResolveContext
	{
		public IMemoryCache MemoryCache { get; } = new MemoryCache(new MemoryCacheOptions());
	}
}
