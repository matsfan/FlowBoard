using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Cards.Delete;

namespace FlowBoard.WebApi.Endpoints.Cards.Delete;

public sealed class DeleteCardEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        AllowAnonymous();
        Summary(s => { s.Summary = "Delete card"; s.Description = "Deletes a card from a column"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || columnId == Guid.Empty || cardId == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var result = await mediator.Send(new DeleteCardCommand(boardId, columnId, cardId), ct);
        if (result.IsFailure)
        {
            // Map not found to 404; else 400
            if (result.Errors.Any(e => e.Code.EndsWith("NotFound")))
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
