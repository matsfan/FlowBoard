using FlowBoard.Domain.Primitives;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.UseCases.Boards.Delete;

public sealed record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;
