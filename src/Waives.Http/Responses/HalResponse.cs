using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class HalResponse
    {
        internal IDictionary<string, HalUri> Links { get; }

        [JsonConstructor]
        internal HalResponse([JsonProperty("_links")] IDictionary<string, HalUri> links)
        {
            Links = links;
        }
    }
}