using FastEndpoints;
using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Application.UseCases.Columns.Handlers;

namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class SetColumnWipLimitEndpoint(SetColumnWipLimitHandler handler) : Endpoint<SetWipLimitRequest>
{
    public override void Configure()
    {
        Post("/boards/{boardId:guid}/columns/{columnId:guid}/wip");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "Set WIP limit"; s.Description = "Sets or clears the WIP limit of a column"; });
    }

    public override async Task HandleAsync(SetWipLimitRequest req, CancellationToken ct)
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
        var result = await handler.HandleAsync(new SetColumnWipLimitCommand(req.BoardId, req.ColumnId, req.WipLimit), ct);
        if (result.IsFailure)
        {
            AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        await Send.OkAsync(cancellation: ct);
    }
}
