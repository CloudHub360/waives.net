using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Client.Responses
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