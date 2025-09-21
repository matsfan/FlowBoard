using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FlowBoard.WebApi.Tests;

public class WebApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Ensure in-memory persistence is used for tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:UseInMemory"] = "true",
                ["Environment"] = "Testing"
            });
        });
    }
}