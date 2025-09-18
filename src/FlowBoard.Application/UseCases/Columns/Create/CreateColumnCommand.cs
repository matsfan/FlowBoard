using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Columns.Create;

public sealed record CreateColumnCommand(Guid BoardId, string Name, int? WipLimit) : IRequest<Result<ColumnDto>>;
