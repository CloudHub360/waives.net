using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Waives.Http
{
    internal class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }

        public JsonContent(string json) :
            base(json, Encoding.UTF8, "application/json")
        { }
    }

    internal static class JsonContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent httpContent)
        {
            var responseStream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false);

            using (var reader = new StreamReader(responseStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var response = new JsonSerializer().Deserialize<T>(jsonTextReader);
                return response;
            }
        }
    }
}