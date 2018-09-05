using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class ApiError
    {
        [JsonProperty]
        internal string Message { get; set; }
    }
}
