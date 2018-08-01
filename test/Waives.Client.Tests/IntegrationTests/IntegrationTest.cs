using MockWaivesApi;
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
        }
    }
}
