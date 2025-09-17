using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Dto;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Handlers;

public sealed class AddCardHandler(IBoardRepository repository, IClock clock) : IRequestHandler<AddCardCommand, Result<CardDto>>
{
    public async Task<Result<CardDto>> HandleAsync(AddCardCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");

        var addResult = board.AddCard(new ColumnId(command.ColumnId), command.Title, command.Description, clock);
        if (addResult.IsFailure)
            return addResult.Errors.ToArray();

        await repository.UpdateAsync(board, ct);
        var card = addResult.Value!;
        return new CardDto(card.Id.Value, card.Title.Value, card.Description.Value, card.Order.Value, card.IsArchived, card.CreatedUtc);
    }

    public Task<Result<CardDto>> Handle(AddCardCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
