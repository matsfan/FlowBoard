using FlowBoard.Application.UseCases.Columns.Dto;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Columns.Queries;

public sealed record GetColumnQuery(Guid BoardId, Guid ColumnId) : IRequest<Result<ColumnDto>>;
