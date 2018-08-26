using System.Net.Http;
using System.Threading.Tasks;

namespace Waives.Http
{
    internal interface IHttpRequestSender
    {
        Task<HttpResponseMessage> Send(HttpRequestMessage request);
    }
}