using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Cards.Dtos;
using FlowBoard.Application.UseCases.Cards.Queries;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class ListCardsHandler(IBoardRepository repository) : IRequestHandler<ListCardsQuery, Result<IReadOnlyCollection<CardDto>>>
{
    public async Task<Result<IReadOnlyCollection<CardDto>>> Handle(ListCardsQuery request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var column = board.Columns.FirstOrDefault(c => c.Id.Value == request.ColumnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var cards = column.Cards
            .OrderBy(c => c.Order.Value)
            .Select(c => new CardDto(c.Id.Value, c.Title.Value, c.Description.Value, c.Order.Value, c.IsArchived, c.CreatedUtc))
            .ToList().AsReadOnly();
        return cards;
    }
}
