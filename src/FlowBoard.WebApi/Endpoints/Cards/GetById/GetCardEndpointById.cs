using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Cards.GetById;

namespace FlowBoard.WebApi.Endpoints.Cards.Get;

public sealed class GetCardEndpointById(IMediator mediator) : EndpointWithoutRequest<GetCardResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        var result = await mediator.Send(new GetCardByIdQuery(boardId, columnId, cardId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var c = result.Value!;
        await Send.OkAsync(new GetCardResponse { Id = c.Id, Title = c.Title, Description = c.Description, Order = c.Order, IsArchived = c.IsArchived, CreatedUtc = c.CreatedUtc, ColumnId = columnId, BoardId = boardId }, ct);
    }
}