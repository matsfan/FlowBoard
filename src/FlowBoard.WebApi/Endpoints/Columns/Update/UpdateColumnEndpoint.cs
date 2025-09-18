using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Columns.Update;

namespace FlowBoard.WebApi.Endpoints.Columns.Update;

public sealed class UpdateColumnEndpoint(IMediator mediator) : Endpoint<UpdateColumnRequest, UpdateColumnResponse>
{
    public override void Configure()
    {
        Put("/boards/{boardId:guid}/columns/{columnId:guid}");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "Update a column"; s.Description = "Full replacement update of column"; });
    }

    public override async Task HandleAsync(UpdateColumnRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        if (boardId == Guid.Empty || columnId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var result = await mediator.Send(new UpdateColumnCommand(boardId, columnId, req.Name, req.Order, req.WipLimit), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var dto = result.Value!;
        await Send.OkAsync(new UpdateColumnResponse { Id = dto.Id, Name = dto.Name, Order = dto.Order, WipLimit = dto.WipLimit }, ct);
    }
}