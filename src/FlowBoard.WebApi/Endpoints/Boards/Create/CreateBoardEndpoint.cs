using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Create;

namespace FlowBoard.WebApi.Endpoints.Boards.Create;

public sealed class CreateBoardEndpoint(IMediator mediator) : Endpoint<CreateBoardRequest, CreateBoardResponse>
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
        var result = await mediator.Send(new CreateBoardCommand(req.Name), ct);
        if (result.IsFailure)
        {
            // For simplicity treat validation/conflict as 400. Could differentiate 409.
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var dto = result.Value!;
        var response = new CreateBoardResponse { Id = dto.Id, Name = dto.Name, CreatedUtc = dto.CreatedUtc };
        await Send.CreatedAtAsync<CreateBoardEndpoint>(new { id = response.Id }, response, generateAbsoluteUrl: false, cancellation: ct);
    }
}
