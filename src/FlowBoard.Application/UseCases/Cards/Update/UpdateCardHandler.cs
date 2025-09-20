using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Cards.Update;

public sealed class UpdateCardHandler(IBoardRepository repository) : IRequestHandler<UpdateCardCommand, Result<CardDto>>
{
    public async Task<Result<CardDto>> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");

        var currentColumnId = new ColumnId(request.CurrentColumnId);
        var desiredColumnId = new ColumnId(request.ColumnId);
        var cardId = new CardId(request.CardId);

        // If column changed -> move (using move semantics with target order). If same -> reorder.
        if (currentColumnId != desiredColumnId)
        {
            // Ensure target order within range; if out of range domain will validate
            var moveResult = board.MoveCard(cardId, currentColumnId, desiredColumnId, request.Order);
            if (moveResult.IsFailure)
                return Result<CardDto>.Failure(moveResult.Errors);
        }
        else
        {
            // Reorder within same column if order differs.
            var column = board.Columns.FirstOrDefault(c => c.Id == currentColumnId);
            if (column is null)
                return Error.NotFound("Column.NotFound", "Column not found");
            var card = column.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card is null)
                return Error.NotFound("Card.NotFound", "Card not found");
            if (card.Order.Value != request.Order)
            {
                var reorderResult = board.ReorderCardWithinColumn(currentColumnId, cardId, request.Order);
                if (reorderResult.IsFailure)
                    return Result<CardDto>.Failure(reorderResult.Errors);
            }
        }

        // After structural moves, rename if needed
        var renameResult = board.RenameCard(desiredColumnId, cardId, request.Title);
        if (renameResult.IsFailure)
            return Result<CardDto>.Failure(renameResult.Errors);

        // Description change
        var descResult = board.ChangeCardDescription(desiredColumnId, cardId, request.Description);
        if (descResult.IsFailure)
            return Result<CardDto>.Failure(descResult.Errors);

        // Archive state sync
        var columnAfter = board.Columns.First(c => c.Id == desiredColumnId);
        var cardAfter = columnAfter.Cards.First(c => c.Id == cardId);
        if (request.IsArchived && !cardAfter.IsArchived)
        {
            var archiveResult = board.ArchiveCard(desiredColumnId, cardId);
            if (archiveResult.IsFailure)
                return Result<CardDto>.Failure(archiveResult.Errors);
        }
        else if (!request.IsArchived && cardAfter.IsArchived)
        {
            var unarchiveResult = board.UnarchiveCard(desiredColumnId, cardId);
            if (unarchiveResult.IsFailure)
                return Result<CardDto>.Failure(unarchiveResult.Errors);
        }

        await repository.UpdateAsync(board, cancellationToken);

        columnAfter = board.Columns.First(c => c.Id == desiredColumnId);
        cardAfter = columnAfter.Cards.First(c => c.Id == cardId);
        return new CardDto(cardAfter.Id.Value, cardAfter.Title.Value, cardAfter.Description.Value, cardAfter.Order.Value, cardAfter.IsArchived, cardAfter.CreatedUtc);
    }
}
