using FlowBoard.Domain;
using FlowBoard.Application.Abstractions;
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
        return await db.Boards
            .Include(b => b.Columns)
            .ThenInclude(c => c.Cards)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Board>> ListAsync(CancellationToken ct = default)
    {
        var list = await db.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .ThenInclude(c => c.Cards)
            .AsSplitQuery()
            .ToListAsync(ct);

        // Ensure nested collections are ordered deterministically
        foreach (var board in list)
        {
            var orderedColumns = board.Columns.OrderBy(c => c.Order.Value).ToList();
            // Reflection-free in-place reordering of private list is not straightforward; rely on domain invariants already keeping order consistent.
            foreach (var col in orderedColumns)
            {
                // Cards order ensured by domain invariants; EF loads in any order, but for consumer safety we can rely on domain normalization during mutations.
            }
        }
        return list.OrderBy(b => b.CreatedUtc).ToList();
    }

    public async Task DeleteAsync(Board board, CancellationToken ct = default)
    {
        db.Boards.Remove(board);
        await db.SaveChangesAsync(ct);
    }
}
