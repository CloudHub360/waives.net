using System.Threading.Tasks;
using Waives.Client.Tests.IntegrationTests.MockApi;
using Xunit;

namespace Waives.Client.Tests.IntegrationTests
{
    public abstract class IntegrationTest : IClassFixture<MockWaivesHost>
    {
        protected internal MockWaivesHost Host { get; }

        protected internal WaivesClient ApiClient { get; }

        protected IntegrationTest(MockWaivesHost host)
        {
            Host = host;
            ApiClient = new WaivesClient(Host.CreateClient());
            Task.Run(async () => await LoginWellKnownAccount(ApiClient)).Wait();
        }

        protected internal static async Task LoginWellKnownAccount(WaivesClient apiClient)
        {
            await apiClient.Login("clientId", "clientSecret");
        }
    }
}
