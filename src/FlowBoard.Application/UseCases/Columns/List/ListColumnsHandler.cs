using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Columns.List;

public sealed class ListColumnsHandler(IBoardRepository repository) : IRequestHandler<ListColumnsQuery, Result<IReadOnlyCollection<ColumnDto>>>
{
    public async Task<Result<IReadOnlyCollection<ColumnDto>>> Handle(ListColumnsQuery request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var columns = board.Columns
            .OrderBy(c => c.Order.Value)
            .Select(c => new ColumnDto(c.Id.Value, c.Name.Value, c.Order.Value, c.WipLimit?.Value))
            .ToList().AsReadOnly();
        return columns;
    }
}
