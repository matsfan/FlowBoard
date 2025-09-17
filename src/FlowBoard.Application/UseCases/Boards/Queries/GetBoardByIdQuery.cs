using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Queries;

public sealed record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDto>>;
