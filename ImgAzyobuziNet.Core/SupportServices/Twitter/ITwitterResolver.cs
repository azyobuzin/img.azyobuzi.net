using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.SupportServices.Twitter
{
    public interface ITwitterResolver
    {
        Task<ImageInfo[]> GetImagesByStatusIdAsync(string statusId);
    }
}
