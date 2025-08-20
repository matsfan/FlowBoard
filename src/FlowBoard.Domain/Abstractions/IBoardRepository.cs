using FlowBoard.Domain.Aggregates;

namespace FlowBoard.Domain.Abstractions;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken ct = default);
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default);
}
