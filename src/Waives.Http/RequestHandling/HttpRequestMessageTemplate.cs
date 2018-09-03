using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Waives.Http.RequestHandling
{
    internal class HttpRequestMessageTemplate
    {
        public HttpRequestMessageTemplate(HttpMethod method, Uri requestUri)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            Headers = new List<KeyValuePair<string, string>>();
        }

        public HttpMethod Method { get; set; }
        public Uri RequestUri { get; set; }
        public HttpContent Content { get; set; }
        public List<KeyValuePair<string, string>> Headers { get; }

        public HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(Method, RequestUri)
            {
                Content = Content
            };

            foreach (var header in Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }
    }
}