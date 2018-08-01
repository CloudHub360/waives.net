using Acheve.AspNetCore.TestHost.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MockWaivesApi
{
    public class MockWaivesApi
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestServerAuthenticationDefaults.AuthenticationScheme;
            }).AddTestServerAuthentication();

            services.AddMvcCore().AddJsonFormatters();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
