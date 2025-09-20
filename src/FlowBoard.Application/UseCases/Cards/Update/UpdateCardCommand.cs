using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Cards.Update;

public sealed record UpdateCardCommand(
    Guid BoardId,
    Guid CurrentColumnId,
    Guid CardId,
    string Title,
    string? Description,
    Guid ColumnId,
    int Order,
    bool IsArchived) : IRequest<Result<CardDto>>;
