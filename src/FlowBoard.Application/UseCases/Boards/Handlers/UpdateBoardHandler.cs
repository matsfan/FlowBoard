using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Commands;
using FlowBoard.Application.UseCases.Boards.Dtos;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Application.UseCases.Boards.Handlers;

public sealed class UpdateBoardHandler(IBoardRepository repository) : IRequestHandler<UpdateBoardCommand, Result<BoardDto>>
{
    public async Task<Result<BoardDto>> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");

        var renameResult = board.Rename(request.Name);
        if (renameResult.IsFailure)
            return Result<BoardDto>.Failure(renameResult.Errors);

        await repository.UpdateAsync(board, cancellationToken);
        return new BoardDto(board.Id.Value, board.Name.Value, board.CreatedUtc);
    }
}
