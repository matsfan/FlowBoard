using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ChangeCardDescriptionEndpoint(ChangeCardDescriptionHandler handler) : Endpoint<ChangeDescriptionRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}/description");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "Change card description"; s.Description = "Updates the description for a card"; });
    }

    public override async Task HandleAsync(ChangeDescriptionRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || columnId == Guid.Empty || cardId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId; req.ColumnId = columnId; req.CardId = cardId;
        var result = await handler.HandleAsync(new ChangeCardDescriptionCommand(req.BoardId, req.ColumnId, req.CardId, req.Description), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
