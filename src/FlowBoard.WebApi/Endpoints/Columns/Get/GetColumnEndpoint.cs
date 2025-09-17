using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Columns.Queries;

namespace FlowBoard.WebApi.Endpoints.Columns.Get;

public sealed class GetColumnEndpoint(IMediator mediator) : EndpointWithoutRequest<GetColumnResponse>
{
    public override void Configure()
    {
        Get("/boards/{boardId:guid}/columns/{columnId:guid}");
        Group<ColumnsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var boardId = Route<Guid>("boardId");
        var columnId = Route<Guid>("columnId");
        var result = await mediator.Send(new GetColumnQuery(boardId, columnId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var c = result.Value!;
        await Send.OkAsync(new GetColumnResponse { Id = c.Id, Name = c.Name, Order = c.Order, WipLimit = c.WipLimit }, ct);
    }
}

public sealed class GetColumnResponse { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }
