using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Cards.Update;

namespace FlowBoard.WebApi.Endpoints.Cards.Update;

public sealed class UpdateCardEndpoint(IMediator mediator) : Endpoint<UpdateCardRequest, UpdateCardResponse>
{
    public override void Configure()
    {
        Put("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "Update card"; s.Description = "Full replacement update of a card."; });
    }

    public override async Task HandleAsync(UpdateCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var currentColumnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || currentColumnId == Guid.Empty || cardId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var result = await mediator.Send(new UpdateCardCommand(boardId, currentColumnId, cardId, req.Title, req.Description, req.ColumnId, req.Order, req.IsArchived), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new UpdateCardResponse
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Order = dto.Order,
            IsArchived = dto.IsArchived,
            CreatedUtc = dto.CreatedUtc,
            BoardId = boardId,
            ColumnId = req.ColumnId
        }, ct);
    }
}