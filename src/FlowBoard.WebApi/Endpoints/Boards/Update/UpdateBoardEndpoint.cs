using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Boards.Commands;

namespace FlowBoard.WebApi.Endpoints.Boards.Update;

public sealed class UpdateBoardEndpoint(IMediator mediator) : Endpoint<UpdateBoardRequest, UpdateBoardResponse>
{
    public override void Configure()
    {
        Put("/boards/{boardId:guid}");
        Group<BoardsGroup>();
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
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new UpdateBoardResponse { Id = dto.Id, Name = dto.Name, CreatedUtc = dto.CreatedUtc }, ct);
    }
}

public sealed class UpdateBoardRequest { public string Name { get; set; } = string.Empty; }
public sealed class UpdateBoardResponse { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public DateTimeOffset CreatedUtc { get; set; } }
