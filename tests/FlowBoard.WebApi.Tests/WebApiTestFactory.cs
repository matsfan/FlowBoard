using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FlowBoard.Infrastructure.Persistence.InMemory;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.WebApi.Tests;

public class WebApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set configuration to use in-memory persistence
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:UseInMemory"] = "true"
            });
        });

        builder.UseEnvironment("Testing");
    }
}