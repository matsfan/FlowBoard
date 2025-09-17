using FastEndpoints;
using FlowBoard.Application.UseCases.Columns.Commands;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class ReorderColumnEndpoint(IMediator mediator) : Endpoint<ReorderColumnRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/reorder");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "Reorder a column"; s.Description = "Changes the order of the column on the board"; });
    }

    public override async Task HandleAsync(ReorderColumnRequest req, CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        if (boardId == Guid.Empty || columnId == Guid.Empty)
        {
            AddError("Invalid route parameters");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req.BoardId = boardId;
        req.ColumnId = columnId;
        var result = await mediator.Send(new ReorderColumnCommand(req.BoardId, req.ColumnId, req.NewOrder), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
