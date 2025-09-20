using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.List;

public sealed record ListBoardsQuery() : IRequest<Result<IReadOnlyCollection<BoardDto>>>;
