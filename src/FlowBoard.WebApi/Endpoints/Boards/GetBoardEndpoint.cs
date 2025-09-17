using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.ValueObjects;

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
        var idValue = Route<Guid>("id");
        if (idValue == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var board = await repository.GetByIdAsync(new BoardId(idValue), ct);
        if (board is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(new CreateBoardResponse { Id = board.Id.Value, Name = board.Name.Value, CreatedUtc = board.CreatedUtc }, ct);
    }
}
