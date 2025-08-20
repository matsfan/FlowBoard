using FlowBoard.Application.Boards.Dtos;
using FlowBoard.Application.Boards.Queries;
using FlowBoard.Domain;

namespace FlowBoard.Application.Boards.Handlers;

public sealed class ListBoardsHandler(IBoardRepository repository)
{
    public async Task<Result<IReadOnlyCollection<BoardDto>>> HandleAsync(Queries.ListBoardsQuery query, CancellationToken ct = default)
    {
        var boards = await repository.ListAsync(ct);
        var dtos = boards
            .OrderBy(b => b.CreatedUtc)
            .Select(b => new BoardDto(b.Id, b.Name, b.CreatedUtc))
            .ToList()
            .AsReadOnly();
        return dtos;
    }
}
