using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class MoveCardEndpoint(MoveCardHandler handler) : Endpoint<MoveCardRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/cards/{cardId:guid}/move");
        Group<CardsGroup>();
        Summary(s =>
        {
            s.Summary = "Move a card across columns";
            s.Description = "Moves an existing card from one column to another at a target order index";
        });
    }

    public override async Task HandleAsync(MoveCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || cardId == Guid.Empty)
        {
            AddError("Route parameters invalid");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId;
        req.CardId = cardId;

        var result = await handler.HandleAsync(new MoveCardCommand(req.BoardId, req.CardId, req.FromColumnId, req.ToColumnId, req.TargetOrder), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
