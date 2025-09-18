using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

using MediatR;

namespace FlowBoard.Application.UseCases.Boards.List;

public sealed class ListBoardsHandler(IBoardRepository repository) : IRequestHandler<ListBoardsQuery, Result<IReadOnlyCollection<BoardDto>>>
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

    // MediatR entrypoint
    public Task<Result<IReadOnlyCollection<BoardDto>>> Handle(ListBoardsQuery request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
