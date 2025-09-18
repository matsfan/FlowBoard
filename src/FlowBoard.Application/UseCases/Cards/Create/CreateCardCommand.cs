using MediatR;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Create;

public sealed record CreateCardCommand(Guid BoardId, Guid ColumnId, string Title, string? Description) : IRequest<Result<CardDto>>;
