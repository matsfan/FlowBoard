using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Cards.List;

public sealed record ListCardsQuery(Guid BoardId, Guid ColumnId) : IRequest<Result<IReadOnlyCollection<CardDto>>>;
