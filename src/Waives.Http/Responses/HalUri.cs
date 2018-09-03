using System;
using System.Linq;
using Newtonsoft.Json;

namespace Waives.Http.Responses
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

        internal Uri CreateUri(object templateTokens = null)
        {
            if (_isTemplated && templateTokens != null)
            {
                return ApplyTemplateTokens(_uri, templateTokens);
            }

            return _uri;
        }

        private static Uri ApplyTemplateTokens(Uri uri, object templateTokens)
        {
            var uriString = uri.ToString();

            uriString = templateTokens.GetType().GetProperties().Aggregate(
                uriString,
                (current, token) => current.Replace($"{{{token.Name}}}", token.GetValue(templateTokens) as string));

            return new Uri(uriString, UriKind.Relative);
        }
    }
}