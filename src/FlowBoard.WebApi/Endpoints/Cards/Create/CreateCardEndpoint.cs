using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Cards.Create;

namespace FlowBoard.WebApi.Endpoints.Cards.Create;

public sealed class CreateCardEndpoint(IMediator mediator) : Endpoint<CreateCardRequest, CreateCardResponse>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards");
        AllowAnonymous();
        Summary(s => { s.Summary = "Create card"; s.Description = "Creates a new card in a column"; });
    }

    public override async Task HandleAsync(CreateCardRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        if (boardId == Guid.Empty || columnId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var result = await mediator.Send(new CreateCardCommand(boardId, columnId, req.Title, req.Description), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.CreatedAtAsync<CreateCardEndpoint>(new { boardId, columnId, cardId = dto.Id }, new CreateCardResponse
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Order = dto.Order,
            IsArchived = dto.IsArchived,
            CreatedUtc = dto.CreatedUtc
        }, generateAbsoluteUrl: false, cancellation: ct);
    }
}
