using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record ReorderCardCommand(Guid BoardId, Guid ColumnId, Guid CardId, int NewOrder) : IRequest<Result>;
