using System.Net.Http;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool allowAutoRedirect = true);
    }
}
