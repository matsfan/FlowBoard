using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Columns.Update;

public sealed record UpdateColumnCommand(Guid BoardId, Guid ColumnId, string Name, int Order, int? WipLimit)
    : IRequest<Result<ColumnDto>>;
