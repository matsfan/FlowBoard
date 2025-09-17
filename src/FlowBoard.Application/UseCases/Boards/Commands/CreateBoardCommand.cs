using MediatR;
using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.Commands;

public sealed record CreateBoardCommand(string Name) : IRequest<Result<BoardDto>>;
