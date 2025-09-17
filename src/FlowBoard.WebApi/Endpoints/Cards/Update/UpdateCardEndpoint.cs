using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Cards.Commands;

namespace FlowBoard.WebApi.Endpoints.Cards.Update;

public sealed class UpdateCardEndpoint(IMediator mediator) : Endpoint<UpdateCardRequest, UpdateCardResponse>
{
    public override void Configure()
    {
        Put("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}");
        Group<CardsGroup>();
        Summary(s => { s.Summary = "Update card"; s.Description = "Full replacement update of a card."; });
    }

    public override async Task HandleAsync(UpdateCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var currentColumnId = Route<Guid>("columnId");
        var cardId = Route<Guid>("cardId");
        if (boardId == Guid.Empty || currentColumnId == Guid.Empty || cardId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var result = await mediator.Send(new UpdateCardCommand(boardId, currentColumnId, cardId, req.Title, req.Description, req.ColumnId, req.Order, req.IsArchived), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new UpdateCardResponse
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Order = dto.Order,
            IsArchived = dto.IsArchived,
            CreatedUtc = dto.CreatedUtc,
            BoardId = boardId,
            ColumnId = req.ColumnId
        }, ct);
    }
}

public sealed class UpdateCardRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ColumnId { get; set; } // Final column id (may differ from route column)
    public int Order { get; set; }
    public bool IsArchived { get; set; }
}

public sealed class UpdateCardResponse
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
