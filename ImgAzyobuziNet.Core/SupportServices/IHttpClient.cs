using System.Net.Http;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.SupportServices
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool allowAutoRedirect = true);
    }
}
