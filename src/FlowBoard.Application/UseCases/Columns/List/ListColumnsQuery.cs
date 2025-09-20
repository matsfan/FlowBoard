using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Columns.List;

public sealed record ListColumnsQuery(Guid BoardId) : IRequest<Result<IReadOnlyCollection<ColumnDto>>>;
