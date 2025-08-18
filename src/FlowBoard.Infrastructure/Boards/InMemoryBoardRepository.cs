using FlowBoard.Domain;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Boards;

internal sealed class InMemoryBoardRepository : IBoardRepository
{
    private readonly ConcurrentDictionary<Guid, Board> _boards = new();

    public Task AddAsync(Board board, CancellationToken ct = default)
    {
        _boards[board.Id] = board;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        var exists = _boards.Values.Any(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _boards.TryGetValue(id, out var board);
        return Task.FromResult(board);
    }

    public Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default)
    {
        IReadOnlyCollection<Board> list = _boards.Values.ToList();
        return Task.FromResult(list);
    }
}

public static class InfrastructureServiceRegistration
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
            // EF Core registration (Sqlite by default)
            var connectionString = configuration?.GetConnectionString("FlowBoard") ?? "Data Source=flowboard.db";
            services.AddDbContext<Infrastructure.Data.FlowBoardDbContext>(o => o.UseSqlite(connectionString));
            services.AddScoped<IBoardRepository, EfBoardRepository>();
        }

        return services;
    }
}

// Marker type for architecture tests
public sealed class InfrastructureAssemblyMarker { }
