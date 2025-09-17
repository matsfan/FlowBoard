using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record DeleteCardCommand(Guid BoardId, Guid ColumnId, Guid CardId) : IRequest<Result>;
