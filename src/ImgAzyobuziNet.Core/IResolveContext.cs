using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
	public interface IResolveContext
	{
		IMemoryCache MemoryCache { get; }
	}
	
	public class DefaultResolveContext : IResolveContext
	{
		public IMemoryCache MemoryCache { get; set; } = new MemoryCache(new MemoryCacheOptions());
	}
}
