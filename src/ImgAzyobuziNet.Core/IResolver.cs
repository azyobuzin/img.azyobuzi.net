using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
	public interface IResolver
	{
		Task<ImageInfo[]> GetImages(Match match);
	}
}
