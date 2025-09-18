using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.GetById;

public sealed record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDto>>;
