using FlowBoard.Application.Boards.Commands;
using FlowBoard.Application.Boards.Dtos;
using FlowBoard.Domain;

namespace FlowBoard.Application.Boards.Handlers;

public sealed class CreateBoardHandler(IBoardRepository repository, IClock clock)
{
    public async Task<Result<BoardDto>> HandleAsync(Commands.CreateBoardCommand command, CancellationToken ct = default)
    {
        if (await repository.ExistsByNameAsync(command.Name.Trim(), ct))
            return Error.Conflict("Board.Name.Duplicate", "A board with that name already exists");

    var boardResult = Board.Create(command.Name, clock);
        if (boardResult.IsFailure)
            return boardResult.Errors.ToArray();

        var board = boardResult.Value!;
        await repository.AddAsync(board, ct);

        return new BoardDto(board.Id, board.Name, board.CreatedUtc);
    }
}
