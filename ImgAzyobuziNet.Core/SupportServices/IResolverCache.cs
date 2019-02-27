using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.SupportServices
{
    public interface IResolverCache
    {
        ValueTask<(bool Exists, T Value)> TryGetValue<T>(string key);
        Task Set(string key, object value);
        Task DeleteExpiredEntries();
    }
}
