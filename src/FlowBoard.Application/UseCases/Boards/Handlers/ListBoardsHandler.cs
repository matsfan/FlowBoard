using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Application.UseCases.Boards.Queries;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.Handlers;

public sealed class ListBoardsHandler(IBoardRepository repository)
{
    public async Task<Result<IReadOnlyCollection<BoardDto>>> HandleAsync(ListBoardsQuery query, CancellationToken ct = default)
    {
        var boards = await repository.ListAsync(ct);
        var dtos = boards
            .OrderBy(b => b.CreatedUtc)
            .Select(b => new BoardDto(b.Id.Value, b.Name.Value, b.CreatedUtc))
            .ToList()
            .AsReadOnly();
        return dtos;
    }
}
