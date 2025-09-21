using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Cards.List;

namespace FlowBoard.WebApi.Endpoints.Cards.List;

public sealed class ListCardsEndpoint(IMediator mediator) : EndpointWithoutRequest<ListCardsResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns/{columnId:guid}/cards");
        AllowAnonymous();
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