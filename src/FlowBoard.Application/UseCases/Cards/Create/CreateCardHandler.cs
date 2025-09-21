using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Cards.Create;
public sealed class CreateCardHandler(IBoardRepository repository, IClock clock, IUserContext userContext) : IRequestHandler<CreateCardCommand, Result<CardDto>>
{
    public async Task<Result<CardDto>> HandleAsync(CreateCardCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var addResult = board.AddCard(new ColumnId(command.ColumnId), command.Title, command.Description, userContext.CurrentUserId, clock);
        if (addResult.IsFailure)
            return addResult.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        var card = addResult.Value!;
        return new CardDto(card.Id.Value, card.Title.Value, card.Description.Value, card.Order.Value, card.IsArchived, card.CreatedUtc);
    }
    public Task<Result<CardDto>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
