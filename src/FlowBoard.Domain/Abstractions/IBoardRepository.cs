namespace FlowBoard.Domain;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken ct = default);
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default);
}
