using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Delete;

public sealed class DeleteCardHandler(IBoardRepository repository) : IRequestHandler<DeleteCardCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteCardCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.DeleteCard(new ColumnId(command.ColumnId), new CardId(command.CardId));
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        return Result.Success();
    }

    public Task<Result> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
