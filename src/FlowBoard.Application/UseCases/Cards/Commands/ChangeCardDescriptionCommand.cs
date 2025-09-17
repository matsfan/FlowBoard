using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record ChangeCardDescriptionCommand(Guid BoardId, Guid ColumnId, Guid CardId, string? Description) : IRequest<Result>;
