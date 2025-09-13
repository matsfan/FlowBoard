using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Infrastructure.Persistence.Ef;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Persistence.Ef.Repositories;

public sealed class EfBoardRepository(FlowBoardDbContext db) : IBoardRepository
{
    public async Task AddAsync(Board board, CancellationToken ct = default)
    {
        await db.Boards.AddAsync(board, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Board board, CancellationToken ct = default)
    {
        db.Boards.Update(board);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await db.Boards.AsNoTracking().AnyAsync(b => b.Name.Value.ToLower() == name.ToLower(), ct);
    }

    public async Task<Board?> GetByIdAsync(BoardId id, CancellationToken ct = default)
    {
        return await db.Boards.FindAsync([id], ct);
    }

    public async Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default)
    {
        var list = await db.Boards.AsNoTracking().ToListAsync(ct);
        return list.OrderBy(b => b.CreatedUtc).ToList();
    }
}
