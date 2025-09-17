using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Cards.Queries;

namespace FlowBoard.WebApi.Endpoints.Cards.Get;

public sealed class GetCardEndpoint(IMediator mediator) : EndpointWithoutRequest<GetCardResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        Group<CardsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        var result = await mediator.Send(new GetCardQuery(boardId, columnId, cardId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var c = result.Value!;
        await Send.OkAsync(new GetCardResponse { Id = c.Id, Title = c.Title, Description = c.Description, Order = c.Order, IsArchived = c.IsArchived, CreatedUtc = c.CreatedUtc, ColumnId = columnId, BoardId = boardId }, ct);
    }
}

public sealed class GetCardResponse { public Guid Id { get; set; } public Guid BoardId { get; set; } public Guid ColumnId { get; set; } public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public int Order { get; set; } public bool IsArchived { get; set; } public DateTimeOffset CreatedUtc { get; set; } }
