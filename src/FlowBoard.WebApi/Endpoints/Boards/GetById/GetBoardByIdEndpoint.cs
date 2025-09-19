using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.WebApi.Endpoints.Boards.GetById;

public sealed class GetBoardByIdEndpoint(IBoardRepository repository) : EndpointWithoutRequest<GetBoardByIdResponse>
{
    public override void Configure()
    {
        Get("/boards/{id:guid}");
        Group<BoardsGroup>();
        AllowAnonymous();
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
        await Send.OkAsync(new GetBoardByIdResponse { Id = board.Id.Value, Name = board.Name.Value, CreatedUtc = board.CreatedUtc }, ct);
    }
}
