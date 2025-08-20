using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Boards;

public sealed class EfBoardRepository(FlowBoardDbContext db) : IBoardRepository
{
    public async Task AddAsync(Board board, CancellationToken ct = default)
    {
        await db.Boards.AddAsync(board, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await db.Boards.AsNoTracking().AnyAsync(b => b.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Boards.FindAsync([id], ct);
    }

    public async Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default)
    {
    // SQLite provider (in-memory mode) cannot translate ORDER BY DateTimeOffset; fetch then order client-side.
    var list = await db.Boards.AsNoTracking().ToListAsync(ct);
    return list.OrderBy(b => b.CreatedUtc).ToList();
    }
}
