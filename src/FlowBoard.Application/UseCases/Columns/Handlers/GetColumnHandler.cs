using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Columns.Dto;
using FlowBoard.Application.UseCases.Columns.Queries;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Columns.Handlers;

public sealed class GetColumnHandler(IBoardRepository repository) : IRequestHandler<GetColumnQuery, Result<ColumnDto>>
{
    public async Task<Result<ColumnDto>> Handle(GetColumnQuery request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var column = board.Columns.FirstOrDefault(c => c.Id.Value == request.ColumnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        return new ColumnDto(column.Id.Value, column.Name.Value, column.Order.Value, column.WipLimit?.Value);
    }
}
