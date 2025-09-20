using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Cards.GetById;

public sealed record GetCardByIdQuery(Guid BoardId, Guid ColumnId, Guid CardId) : IRequest<Result<CardDto>>;
