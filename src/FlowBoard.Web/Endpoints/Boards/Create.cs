using FastEndpoints;
using FlowBoard.Application.Boards;
using FlowBoard.Domain;

namespace FlowBoard.Web.Endpoints.Boards;

public sealed class CreateBoardRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateBoardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class CreateBoardEndpoint(ICreateBoardHandler handler) : Endpoint<CreateBoardRequest, CreateBoardResponse>
{
    public override void Configure()
    {
        Post("/boards");
        Group<BoardsGroup>();
        Summary(s =>
        {
            s.Summary = "Create a new board";
            s.Description = "Creates a new board with the given name";
        });
    }

    public override async Task HandleAsync(CreateBoardRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new CreateBoardCommand(req.Name), ct);
        if (result.IsFailure)
        {
            // For simplicity treat validation/conflict as 400. Could differentiate 409.
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var dto = result.Value!;
        var response = new CreateBoardResponse { Id = dto.Id, Name = dto.Name, CreatedUtc = dto.CreatedUtc };
        await SendCreatedAtAsync<GetBoardEndpoint>(new { id = response.Id }, response, generateAbsoluteUrl: false, cancellation: ct);
    }
}

public sealed class GetBoardEndpoint(IBoardRepository repository) : EndpointWithoutRequest<CreateBoardResponse>
{
    public override void Configure()
    {
        Get("/boards/{id:guid}");
        Group<BoardsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        if (id == Guid.Empty)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        var board = await repository.GetByIdAsync(id, ct);
        if (board is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        await SendAsync(new CreateBoardResponse { Id = board.Id, Name = board.Name, CreatedUtc = board.CreatedUtc }, cancellation: ct);
    }
}

public sealed class BoardsGroup : Group
{
    public BoardsGroup()
    {
        Configure("/", ep =>
        {
            ep.Description(x => x.WithGroupName("Boards"));
        });
    }
}
