using FastEndpoints;
using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;

namespace FlowBoard.WebApi.Endpoints.Boards;

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
