using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Application.UseCases.Columns.Dto;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Columns.Handlers;

public sealed class AddColumnHandler(IBoardRepository repository)
{
    public async Task<Result<ColumnDto>> HandleAsync(AddColumnCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");

        var result = board.AddColumn(command.Name, command.WipLimit);
        if (result.IsFailure)
            return result.Errors.ToArray();

        await repository.UpdateAsync(board, ct);
        var column = result.Value!;
        return new ColumnDto(column.Id.Value, column.Name.Value, column.Order.Value, column.WipLimit?.Value);
    }
}
