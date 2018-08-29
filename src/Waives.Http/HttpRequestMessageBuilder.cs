using System.Net.Http;

namespace Waives.Http
{
    internal static class HttpRequestMessageBuilder
    {
        public static HttpRequestMessage BuildRequest(HttpRequestMessageTemplate template)
        {
            var request = new HttpRequestMessage(template.Method, template.RequestUri)
            {
                Content = template.Content
            };

            foreach (var header in template.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }
    }
}