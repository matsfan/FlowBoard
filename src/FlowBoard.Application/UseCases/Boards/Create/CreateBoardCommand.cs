using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.Create;

public sealed record CreateBoardCommand(string Name) : IRequest<Result<BoardDto>>;
