using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Waives.Http
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
    }
}