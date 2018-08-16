using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class DocumentCollection
    {
        [JsonProperty("documents")]
        internal IEnumerable<HalResponse> Documents { get; set; }
    }
}
