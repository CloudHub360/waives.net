using System;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http;

namespace Waives.Reactive
{
    public static class WaivesApi
    {
        internal static WaivesClient ApiClient { get; private set; } = new WaivesClient();

        public static async Task<WaivesClient> Login(
            string clientId, string clientSecret,
            string apiUri = "https://api.waives.io/")
        {
            if (string.IsNullOrWhiteSpace(apiUri))
            {
                throw new ArgumentNullException(nameof(apiUri));
            }

            return await Login(clientId, clientSecret, new Uri(apiUri)).ConfigureAwait(false);
        }

        public static async Task<WaivesClient> Login(string clientId, string clientSecret, Uri apiUri)
        {
            ApiClient = new WaivesClient(new HttpClient { BaseAddress = apiUri });
            await ApiClient.Login(clientId, clientSecret).ConfigureAwait(false);

            return ApiClient;
        }

        public static PipelineBuilder CreatePipeline()
        {
            return new PipelineBuilder();
        }
    }
}