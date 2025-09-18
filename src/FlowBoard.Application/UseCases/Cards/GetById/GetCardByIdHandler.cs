using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.GetById;

public sealed class GetCardByIdHandler(IBoardRepository repository) : IRequestHandler<GetCardByIdQuery, Result<CardDto>>
{
    public async Task<Result<CardDto>> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var column = board.Columns.FirstOrDefault(c => c.Id.Value == request.ColumnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.Cards.FirstOrDefault(c => c.Id.Value == request.CardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found");
        return new CardDto(card.Id.Value, card.Title.Value, card.Description.Value, card.Order.Value, card.IsArchived, card.CreatedUtc);
    }
}
