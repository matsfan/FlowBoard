using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Abstractions;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken ct = default);
    Task<Board?> GetByIdAsync(BoardId id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default); // keep string for query simplicity
    Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default);
}
