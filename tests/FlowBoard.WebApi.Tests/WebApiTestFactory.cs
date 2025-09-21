using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FlowBoard.Infrastructure.Persistence.InMemory;
using FlowBoard.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using FlowBoard.Infrastructure.Persistence.Ef;

namespace FlowBoard.WebApi.Tests;

public class WebApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<FlowBoardDbContext>) ||
                                                  d.ServiceType == typeof(FlowBoardDbContext)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
            
            // Use in-memory database for testing with unique name
            services.AddDbContext<FlowBoardDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}