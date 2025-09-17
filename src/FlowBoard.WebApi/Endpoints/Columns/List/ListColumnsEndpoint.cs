using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Columns.Queries;

namespace FlowBoard.WebApi.Endpoints.Columns.List;

public sealed class ListColumnsEndpoint(IMediator mediator) : EndpointWithoutRequest<ListColumnsResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns");
        Group<ColumnsGroup>();
        Summary(s => { s.Summary = "List columns"; s.Description = "Lists all columns for a board"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var result = await mediator.Send(new ListColumnsQuery(boardId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var response = new ListColumnsResponse
        {
            Columns = result.Value!.Select(c => new ListColumnsResponse.ColumnItem
            {
                Id = c.Id,
                Name = c.Name,
                Order = c.Order,
                WipLimit = c.WipLimit
            }).ToList()
        };
        await Send.OkAsync(response, ct);
    }
}

public sealed class ListColumnsResponse
{
    public List<ColumnItem> Columns { get; set; } = [];
    public sealed class ColumnItem { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }
}
