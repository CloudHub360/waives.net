using System.Net.Http;
using System.Threading.Tasks;

namespace Waives.Http.RequestHandling
{
    internal interface IHttpRequestSender
    {
        int Timeout { get; set; }

        void Authenticate(string accessToken);

        Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request);
    }
}