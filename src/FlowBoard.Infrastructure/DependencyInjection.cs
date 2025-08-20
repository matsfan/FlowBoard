using FlowBoard.Domain;
using FlowBoard.Infrastructure.Persistence.Ef;
using FlowBoard.Infrastructure.Persistence.Ef.Repositories;
using FlowBoard.Infrastructure.Persistence.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddSingleton<IClock, SystemClock>();

        var useInMemory = false;
        var raw = configuration?["Persistence:UseInMemory"];
        if (bool.TryParse(raw, out var parsed)) useInMemory = parsed;
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
