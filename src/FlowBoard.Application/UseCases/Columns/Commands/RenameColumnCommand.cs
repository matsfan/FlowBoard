using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Columns.Commands;

public sealed record RenameColumnCommand(Guid BoardId, Guid ColumnId, string Name) : IRequest<Result>;
