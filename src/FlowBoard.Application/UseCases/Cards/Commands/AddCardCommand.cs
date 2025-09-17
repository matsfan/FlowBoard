using MediatR;
using FlowBoard.Application.UseCases.Cards.Dto;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record AddCardCommand(Guid BoardId, Guid ColumnId, string Title, string? Description) : IRequest<Result<CardDto>>;
