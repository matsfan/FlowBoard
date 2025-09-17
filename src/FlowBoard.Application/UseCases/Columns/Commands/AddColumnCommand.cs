using MediatR;
using FlowBoard.Application.UseCases.Columns.Dto;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Columns.Commands;

public sealed record AddColumnCommand(Guid BoardId, string Name, int? WipLimit) : IRequest<Result<ColumnDto>>;
