using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Application.UseCases.Boards.Queries;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Handlers;

public sealed class GetBoardByIdHandler(IBoardRepository repository) : IRequestHandler<GetBoardByIdQuery, Result<BoardDto>>
{
    public async Task<Result<BoardDto>> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        return new BoardDto(board.Id.Value, board.Name.Value, board.CreatedUtc);
    }
}
