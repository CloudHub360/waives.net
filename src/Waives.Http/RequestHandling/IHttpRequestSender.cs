using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Responses;

namespace Waives.Http.RequestHandling
{
    internal interface IHttpRequestSender
    {
        int Timeout { get; set; }

        void Authenticate(AccessToken accessToken);

        Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request);
    }
}