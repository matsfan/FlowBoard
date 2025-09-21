using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Delete;

namespace FlowBoard.WebApi.Endpoints.Boards.Delete;

public sealed class DeleteBoardEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{boardId:guid}");
        Group<BoardsGroup>();
        AllowAnonymous();
        Summary(s => { s.Summary = "Delete a board"; s.Description = "Deletes a board by id."; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        if (boardId == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var result = await mediator.Send(new DeleteBoardCommand(boardId), ct);
        if (result.IsFailure)
        {
            // Map not found to 404; else 400
            if (result.Errors.Any(e => e.Code == "Board.NotFound"))
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
