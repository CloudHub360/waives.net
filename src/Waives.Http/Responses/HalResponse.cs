using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class HalResponse
    {
        internal IDictionary<string, HalUri> Links { get; }
        internal string Id { get; }

        [JsonConstructor]
        internal HalResponse([JsonProperty("_links")] IDictionary<string, HalUri> links, [JsonProperty("id")] string id)
        {
            Links = links;
            Id = id;
        }
    }
}