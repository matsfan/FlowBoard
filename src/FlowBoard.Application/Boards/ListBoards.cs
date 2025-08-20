using FlowBoard.Domain;

namespace FlowBoard.Application.Boards;

public sealed class ListBoardsHandler(IBoardRepository repository) : IListBoardsHandler
{
    public async Task<Result<IReadOnlyCollection<BoardDto>>> HandleAsync(ListBoardsQuery query, CancellationToken ct = default)
    {
        var boards = await repository.ListAsync(ct);
        var dtos = boards
            .OrderBy(b => b.CreatedUtc) // ensure deterministic ordering
            .Select(b => new BoardDto(b.Id, b.Name, b.CreatedUtc))
            .ToList()
            .AsReadOnly();
        return dtos;
    }
}
