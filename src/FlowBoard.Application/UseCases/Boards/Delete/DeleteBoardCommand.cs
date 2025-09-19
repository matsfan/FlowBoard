using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Delete;

public sealed record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;
