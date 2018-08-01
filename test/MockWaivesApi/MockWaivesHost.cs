using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Acheve.AspNetCore.TestHost.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace MockWaivesApi
{
    public class MockWaivesHost
    {
        private readonly TestServer _server;

        public MockWaivesHost()
        {
            _server = new TestServer(
                new WebHostBuilder()
                    .UseStartup<MockWaivesApi>());
        }

        public HttpClient CreateClient() => _server.CreateClient().WithDefaultIdentity(Identities.User);
    }

    internal static class Identities
    {
        internal static readonly IEnumerable<Claim> User = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "User")
        };

        public static readonly IEnumerable<Claim> Empty = Enumerable.Empty<Claim>();
    }
}
