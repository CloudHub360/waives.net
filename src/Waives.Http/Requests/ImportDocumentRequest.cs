using Newtonsoft.Json;

namespace Waives.Http.Requests
{
    public class ImportDocumentRequest
    {
        [JsonProperty("url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}