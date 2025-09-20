using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Delete;

public sealed record DeleteCardCommand(Guid BoardId, Guid ColumnId, Guid CardId) : IRequest<Result>;
