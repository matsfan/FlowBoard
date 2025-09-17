using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class MoveCardHandler(IBoardRepository repository) : IRequestHandler<MoveCardCommand, Result>
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

    public Task<Result> Handle(MoveCardCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
