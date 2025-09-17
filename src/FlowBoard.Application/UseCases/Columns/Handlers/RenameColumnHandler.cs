using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

using MediatR;

namespace FlowBoard.Application.UseCases.Columns.Handlers;

public sealed class RenameColumnHandler(IBoardRepository repository) : IRequestHandler<RenameColumnCommand, Result>
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

    public Task<Result> Handle(RenameColumnCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
