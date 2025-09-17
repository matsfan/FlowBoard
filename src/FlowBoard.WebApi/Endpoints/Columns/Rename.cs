using FastEndpoints;
using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Application.UseCases.Columns.Handlers;

namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class RenameColumnEndpoint(RenameColumnHandler handler) : Endpoint<RenameColumnRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/rename");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "Rename a column"; s.Description = "Renames the specified column"; });
    }

    public override async Task HandleAsync(RenameColumnRequest req, CancellationToken ct)
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
        var result = await handler.HandleAsync(new RenameColumnCommand(req.BoardId, req.ColumnId, req.Name), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
