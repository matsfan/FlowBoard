using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Boards.GetById;

public sealed record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDto>>;
