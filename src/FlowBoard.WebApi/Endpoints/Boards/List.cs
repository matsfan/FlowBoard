using FastEndpoints;
using FlowBoard.Application.Boards.Handlers;
using FlowBoard.Application.Boards.Queries;

namespace FlowBoard.WebApi.Endpoints.Boards;

public sealed class ListBoardsResponse
{
    public IReadOnlyCollection<BoardItem> Boards { get; set; } = Array.Empty<BoardItem>();

    public sealed class BoardItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedUtc { get; set; }
    }
}

public sealed class ListBoardsEndpoint(ListBoardsHandler handler) : EndpointWithoutRequest<ListBoardsResponse>
{
    public override void Configure()
    {
        Get("/boards");
        Group<BoardsGroup>();
        Summary(s =>
        {
            s.Summary = "List all boards";
            s.Description = "Returns all boards in creation order";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await handler.HandleAsync(new ListBoardsQuery(), ct);
        if (result.IsFailure)
        {
            // Should not normally fail; treat as 400 for consistency (validation) though no validation currently.
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await SendErrorsAsync(cancellation: ct);
            return;
        }
        var items = result.Value!.Select(b => new ListBoardsResponse.BoardItem
        {
            Id = b.Id,
            Name = b.Name,
            CreatedUtc = b.CreatedUtc
        }).ToList();

        await SendAsync(new ListBoardsResponse { Boards = items }, cancellation: ct);
    }
}
