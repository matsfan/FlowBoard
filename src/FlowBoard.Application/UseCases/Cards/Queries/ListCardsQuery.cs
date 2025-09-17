using FlowBoard.Application.UseCases.Cards.Dtos;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Queries;

public sealed record ListCardsQuery(Guid BoardId, Guid ColumnId) : IRequest<Result<IReadOnlyCollection<CardDto>>>;
