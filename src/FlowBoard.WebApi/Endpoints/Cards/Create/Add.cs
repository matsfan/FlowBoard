using FastEndpoints;
using FlowBoard.Application.UseCases.Cards.Commands;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class AddCardEndpoint(IMediator mediator) : Endpoint<AddCardRequest, AddCardResponse>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards");
        Group<CardsGroup>();
        Summary(s =>
        {
            s.Summary = "Add a card to a column";
            s.Description = "Creates a new card in the specified column";
        });
    }

    public override async Task HandleAsync(AddCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        if (boardId == Guid.Empty || columnId == Guid.Empty)
        {
            AddError("Route parameters invalid");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId;
        req.ColumnId = columnId;
        var result = await mediator.Send(new AddCardCommand(req.BoardId, req.ColumnId, req.Title, req.Description), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var dto = result.Value!;
        var response = new AddCardResponse
        {
            Id = dto.Id,
            BoardId = req.BoardId,
            ColumnId = req.ColumnId,
            Title = dto.Title,
            Description = dto.Description,
            Order = dto.Order,
            CreatedUtc = dto.CreatedUtc
        };
        await Send.CreatedAtAsync<AddCardEndpoint>(new { boardId = req.BoardId, columnId = req.ColumnId }, response, generateAbsoluteUrl: false, cancellation: ct);
    }
}
