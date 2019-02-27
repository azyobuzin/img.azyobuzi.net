using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.SupportServices
{
    internal class NoResolverCache : IResolverCache
    {
        public ValueTask<(bool Exists, T Value)> TryGetValue<T>(string key)
        {
            return new ValueTask<(bool, T)>((false, default));
        }

        public Task Set(string key, object value)
        {
            return Task.CompletedTask;
        }

        public Task DeleteExpiredEntries()
        {
            return Task.CompletedTask;
        }
    }
}
