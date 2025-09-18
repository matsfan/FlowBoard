using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Update;

public sealed record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result<BoardDto>>;
