using MediatR;
using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.Queries;

public sealed record ListBoardsQuery() : IRequest<Result<IReadOnlyCollection<BoardDto>>>;
