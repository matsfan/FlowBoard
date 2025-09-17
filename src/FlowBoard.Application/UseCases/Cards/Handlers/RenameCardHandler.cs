using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class RenameCardHandler(IBoardRepository repository)
{
    public async Task<Result> HandleAsync(RenameCardCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.RenameCard(new ColumnId(command.ColumnId), new CardId(command.CardId), command.Title);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }
}
