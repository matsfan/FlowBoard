using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Boards.Update;

public sealed record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result<BoardDto>>;
