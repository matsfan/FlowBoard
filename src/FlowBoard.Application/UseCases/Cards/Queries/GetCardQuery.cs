using FlowBoard.Application.UseCases.Cards.Dtos;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Queries;

public sealed record GetCardQuery(Guid BoardId, Guid ColumnId, Guid CardId) : IRequest<Result<CardDto>>;
