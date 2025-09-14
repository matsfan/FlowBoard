using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Columns.Handlers;

public sealed class ReorderColumnHandler(IBoardRepository repository)
{
    public async Task<Result> HandleAsync(ReorderColumnCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.ReorderColumn(new ColumnId(command.ColumnId), command.NewOrder);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }
}
