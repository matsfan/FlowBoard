using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Commands;

// Strict full replacement for now only supports Name as mutable property.
public sealed record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result<BoardDto>>;
