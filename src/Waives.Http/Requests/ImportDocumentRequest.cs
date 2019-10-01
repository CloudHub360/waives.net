using System;
using Newtonsoft.Json;

namespace Waives.Http.Requests
{
    public class ImportDocumentRequest
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}