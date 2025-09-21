using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Update;

namespace FlowBoard.WebApi.Endpoints.Boards.Update;

public sealed class UpdateBoardEndpoint(IMediator mediator) : Endpoint<UpdateBoardRequest, UpdateBoardResponse>
{
    public override void Configure()
    {
        Put("/{boardId:guid}");
        Group<BoardsGroup>();
        AllowAnonymous();
        Summary(s => { s.Summary = "Update a board"; s.Description = "Full replacement update of a board (currently only name)"; });
    }

    public override async Task HandleAsync(UpdateBoardRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("boardId");
        if (id == Guid.Empty)
        {
            AddError("Invalid boardId");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var result = await mediator.Send(new UpdateBoardCommand(id, req.Name), ct);
        if (result.IsFailure)
        {
            // Map specific error types to HTTP status codes
            if (result.Errors.Any(e => e.Type == FlowBoard.Domain.Primitives.ErrorType.NotFound))
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            if (result.Errors.Any(e => e.Type == FlowBoard.Domain.Primitives.ErrorType.Forbidden))
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            // Default to 400 for validation/conflict errors
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new UpdateBoardResponse { Id = dto.Id, Name = dto.Name, CreatedUtc = dto.CreatedUtc }, ct);
    }
}