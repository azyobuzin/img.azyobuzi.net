using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
	public interface IResolver
	{
		string ServiceId { get; }
		string ServiceName { get; }
		string Pattern { get; }
		Task<ImageInfo[]> GetImages(IResolveContext context, Match match);
	}
}
