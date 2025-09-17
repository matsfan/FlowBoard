using FlowBoard.Application.UseCases.Columns.Dto;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Columns.Commands;

// Full replacement: client supplies final name, order, wipLimit.
public sealed record UpdateColumnCommand(Guid BoardId, Guid ColumnId, string Name, int Order, int? WipLimit)
    : IRequest<Result<ColumnDto>>;
