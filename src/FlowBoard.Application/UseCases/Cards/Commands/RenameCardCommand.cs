using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record RenameCardCommand(Guid BoardId, Guid ColumnId, Guid CardId, string Title) : IRequest<Result>;
