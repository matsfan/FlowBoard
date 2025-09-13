using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ReorderCardEndpoint(ReorderCardHandler handler) : Endpoint<ReorderCardRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}/reorder");
        Group<CardsGroup>();
        Summary(s =>
        {
            s.Summary = "Reorder a card within a column";
            s.Description = "Changes the order index of a card within the same column";
        });
    }

    public override async Task HandleAsync(ReorderCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || columnId == Guid.Empty || cardId == Guid.Empty)
        {
            AddError("Route parameters invalid");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId;
        req.ColumnId = columnId;
        req.CardId = cardId;

        var result = await handler.HandleAsync(new ReorderCardCommand(req.BoardId, req.ColumnId, req.CardId, req.NewOrder), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
