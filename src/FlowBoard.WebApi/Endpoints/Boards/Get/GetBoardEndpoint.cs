using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Boards.Queries;

namespace FlowBoard.WebApi.Endpoints.Boards.Get;

public sealed class GetBoardEndpoint(IMediator mediator) : EndpointWithoutRequest<GetBoardResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}");
        Group<BoardsGroup>();
        Summary(s => { s.Summary = "Get a board"; s.Description = "Returns a board by id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("boardId");
        if (id == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var result = await mediator.Send(new GetBoardByIdQuery(id), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new GetBoardResponse { Id = dto.Id, Name = dto.Name, CreatedUtc = dto.CreatedUtc }, ct);
    }
}

public sealed class GetBoardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}
