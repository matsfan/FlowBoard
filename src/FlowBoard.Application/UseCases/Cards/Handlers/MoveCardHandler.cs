using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class MoveCardHandler(IBoardRepository repository)
{
    public async Task<Result> HandleAsync(MoveCardCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");

        var result = board.MoveCard(new CardId(command.CardId), new ColumnId(command.FromColumnId), new ColumnId(command.ToColumnId), command.TargetOrder);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }
}
