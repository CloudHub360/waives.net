using Acheve.AspNetCore.TestHost.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Waives.Client.Tests.IntegrationTests.MockApi
{
    public class MockWaivesApi
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestServerAuthenticationDefaults.AuthenticationScheme;
            }).AddTestServerAuthentication();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
        }
    }
}
