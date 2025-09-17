using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Cards.Queries;

namespace FlowBoard.WebApi.Endpoints.Cards.List;

public sealed class ListCardsEndpoint(IMediator mediator) : EndpointWithoutRequest<ListCardsResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns/{columnId:guid}/cards");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "List cards"; s.Description = "Lists all cards in a column"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var result = await mediator.Send(new ListCardsQuery(boardId, columnId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var response = new ListCardsResponse
        {
            Cards = result.Value!.Select(c => new ListCardsResponse.CardItem
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Order = c.Order,
                IsArchived = c.IsArchived,
                CreatedUtc = c.CreatedUtc
            }).ToList()
        };
        await Send.OkAsync(response, ct);
    }
}

public sealed class ListCardsResponse
{
    public List<CardItem> Cards { get; set; } = [];
    public sealed class CardItem { public Guid Id { get; set; } public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public int Order { get; set; } public bool IsArchived { get; set; } public DateTimeOffset CreatedUtc { get; set; } }
}
