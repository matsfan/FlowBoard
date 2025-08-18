using FlowBoard.Domain;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IBoardRepository, InMemoryBoardRepository>();
        return services;
    }
}

// Marker type for architecture tests
public sealed class InfrastructureAssemblyMarker { }
