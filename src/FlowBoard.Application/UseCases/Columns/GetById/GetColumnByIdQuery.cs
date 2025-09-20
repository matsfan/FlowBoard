using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Columns.GetById;

public sealed record GetColumnByIdQuery(Guid BoardId, Guid ColumnId) : IRequest<Result<ColumnDto>>;
