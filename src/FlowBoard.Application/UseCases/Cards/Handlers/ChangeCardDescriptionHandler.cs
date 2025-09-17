using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class ChangeCardDescriptionHandler(IBoardRepository repository)
{
    public async Task<Result> HandleAsync(ChangeCardDescriptionCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.ChangeCardDescription(new ColumnId(command.ColumnId), new CardId(command.CardId), command.Description);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }
}
