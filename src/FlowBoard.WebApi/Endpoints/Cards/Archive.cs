using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ArchiveCardEndpoint(IMediator mediator) : Endpoint<ArchiveCardRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}/archive");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "Archive a card"; s.Description = "Archives the specified card"; });
    }

    public override async Task HandleAsync(ArchiveCardRequest req, CancellationToken ct)
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
        req.BoardId = boardId;
        req.ColumnId = columnId;
        req.CardId = cardId;
        var result = await mediator.Send(new ArchiveCardCommand(req.BoardId, req.ColumnId, req.CardId), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
