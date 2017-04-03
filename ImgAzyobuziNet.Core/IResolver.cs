using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
    public interface IResolver
    {
        ValueTask<ImageInfo[]> GetImages(Match match);
    }
}
