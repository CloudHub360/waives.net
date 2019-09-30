using System.Net.Http;
using System.Threading.Tasks;

namespace Waives.Http.RequestHandling
{
    internal interface IHttpRequestSender
    {
        int Timeout { get; set; }

        Task<HttpResponseMessage> SendAsync(HttpRequestMessageTemplate request);
    }
}