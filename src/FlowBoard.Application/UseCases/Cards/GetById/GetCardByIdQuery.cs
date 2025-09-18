using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.GetById;

public sealed record GetCardByIdQuery(Guid BoardId, Guid ColumnId, Guid CardId) : IRequest<Result<CardDto>>;
