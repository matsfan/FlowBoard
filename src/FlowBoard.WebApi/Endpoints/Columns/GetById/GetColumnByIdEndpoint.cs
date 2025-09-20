using FastEndpoints;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Columns.GetById;

namespace FlowBoard.WebApi.Endpoints.Columns.GetById;

public sealed class GetColumnEndpoint(IMediator mediator) : EndpointWithoutRequest<GetColumnByIdResponse>
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
        var result = await mediator.Send(new GetColumnByIdQuery(boardId, columnId), ct);
        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var c = result.Value!;
        await Send.OkAsync(new GetColumnByIdResponse { Id = c.Id, Name = c.Name, Order = c.Order, WipLimit = c.WipLimit }, ct);
    }
}