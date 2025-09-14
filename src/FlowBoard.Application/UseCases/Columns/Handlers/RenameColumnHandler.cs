using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Columns.Handlers;

public sealed class RenameColumnHandler(IBoardRepository repository)
{
    public async Task<Result> HandleAsync(RenameColumnCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.RenameColumn(new ColumnId(command.ColumnId), command.Name);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }
}
