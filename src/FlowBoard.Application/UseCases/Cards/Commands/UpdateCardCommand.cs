using FlowBoard.Application.UseCases.Cards.Dtos;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Cards.Commands;

// Full replacement: client supplies final Title, Description, ColumnId (final), Order, Archive state.
public sealed record UpdateCardCommand(
    Guid BoardId,
    Guid CurrentColumnId,
    Guid CardId,
    string Title,
    string? Description,
    Guid ColumnId,
    int Order,
    bool IsArchived) : IRequest<Result<CardDto>>;
