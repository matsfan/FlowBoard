using FastEndpoints;
using FlowBoard.Application.UseCases.Columns.Commands;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class AddColumnEndpoint(IMediator mediator) : Endpoint<AddColumnRequest, AddColumnResponse>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "Add a column"; s.Description = "Creates a new column on the board"; });
    }

    public override async Task HandleAsync(AddColumnRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        if (boardId == Guid.Empty)
        {
            AddError("Invalid boardId");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId;
        var result = await mediator.Send(new AddColumnCommand(req.BoardId, req.Name, req.WipLimit), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        var response = new AddColumnResponse { Id = dto.Id, BoardId = req.BoardId, Name = dto.Name, Order = dto.Order, WipLimit = dto.WipLimit };
        await Send.CreatedAtAsync<AddColumnEndpoint>(new { boardId = req.BoardId }, response, generateAbsoluteUrl: false, cancellation: ct);
    }
}
