using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.List;

namespace FlowBoard.WebApi.Endpoints.Boards.List;

public sealed class ListBoardsEndpoint(IMediator mediator) : EndpointWithoutRequest<ListBoardsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<BoardsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all boards";
            s.Description = "Returns all boards in creation order";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListBoardsQuery(), ct);
        if (result.IsFailure)
        {
            // Should not normally fail; treat as 400 for consistency (validation) though no validation currently.
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var items = result.Value!.Select(b => new ListBoardsResponse.BoardItem
        {
            Id = b.Id,
            Name = b.Name,
            CreatedUtc = b.CreatedUtc
        }).ToList();

        await Send.OkAsync(new ListBoardsResponse { Boards = items }, cancellation: ct);
    }
}
