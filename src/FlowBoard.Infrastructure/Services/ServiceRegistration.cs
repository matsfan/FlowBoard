using FlowBoard.Domain;
using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence.Ef;
using FlowBoard.Infrastructure.Persistence.Ef.Repositories;
using FlowBoard.Infrastructure.Persistence.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlowBoard.Domain.Abstractions;

namespace FlowBoard.Infrastructure.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddSingleton<IClock, SystemClock>();

        var useInMemory = false;
        var raw = configuration?["Persistence:UseInMemory"];
        if (bool.TryParse(raw, out var parsed)) useInMemory = parsed;
        
        // Check for test environment more precisely
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isTestEnvironment = environment == "Testing" || 
                               configuration?["Environment"] == "Testing" ||
                               // Only consider as test if we're running from a test assembly (not just any assembly with "Test" in name)
                               AppDomain.CurrentDomain.GetAssemblies()
                                   .Any(a => a.FullName?.StartsWith("Microsoft.TestPlatform") == true ||
                                            a.FullName?.StartsWith("xunit") == true ||
                                            a.FullName?.StartsWith("NUnit") == true);
        
        if (isTestEnvironment)
        {
            useInMemory = true;
        }
        
        if (useInMemory)
        {
            services.AddSingleton<IBoardRepository, InMemoryBoardRepository>();
        }
        else
        {
            var connectionString = configuration?.GetConnectionString("FlowBoard") ?? "Data Source=flowboard.db";
            services.AddDbContext<FlowBoardDbContext>(o => o.UseSqlite(connectionString));
            services.AddScoped<IBoardRepository, EfBoardRepository>();
        }

        return services;
    }
}

// Marker type for architecture tests
public sealed class InfrastructureAssemblyMarker { }
