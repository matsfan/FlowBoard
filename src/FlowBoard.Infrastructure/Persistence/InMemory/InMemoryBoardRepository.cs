using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace FlowBoard.Infrastructure.Persistence.InMemory;

internal sealed class InMemoryBoardRepository : IBoardRepository
{
    private readonly ConcurrentDictionary<BoardId, Board> _boards = new();

    public Task AddAsync(Board board, CancellationToken ct = default)
    {
        _boards[board.Id] = board;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Board board, CancellationToken ct = default)
    {
        _boards[board.Id] = board; // overwrite
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        var exists = _boards.Values.Any(b => string.Equals(b.Name.Value, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<Board?> GetByIdAsync(BoardId id, CancellationToken ct = default)
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
