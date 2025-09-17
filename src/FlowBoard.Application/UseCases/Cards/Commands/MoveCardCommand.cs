using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record MoveCardCommand(Guid BoardId, Guid CardId, Guid FromColumnId, Guid ToColumnId, int TargetOrder) : IRequest<Result>;
