using System;
using Newtonsoft.Json;

namespace Waives.Client.Responses
{
    internal class HalUri
    {
        private readonly Uri _uri;
        private readonly bool _isTemplated;

        [JsonConstructor]
        internal HalUri(
            [JsonProperty("href", Required = Required.Always)]Uri uri,
            [JsonProperty("templated")]bool templated)
        {
            _uri = uri;
            _isTemplated = templated;
        }

        internal Uri CreateUri(params string[] templateTokens)
        {
            return _uri;
        }
    }
}