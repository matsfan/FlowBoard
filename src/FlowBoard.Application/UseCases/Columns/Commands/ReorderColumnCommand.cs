using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Columns.Commands;

public sealed record ReorderColumnCommand(Guid BoardId, Guid ColumnId, int NewOrder) : IRequest<Result>;
