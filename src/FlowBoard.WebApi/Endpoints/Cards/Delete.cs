using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class DeleteCardEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "Delete a card"; s.Description = "Deletes the specified card from a column"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
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
        var result = await mediator.Send(new DeleteCardCommand(boardId, columnId, cardId), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
