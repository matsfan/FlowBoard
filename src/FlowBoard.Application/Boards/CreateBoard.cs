using FlowBoard.Domain;

namespace FlowBoard.Application.Boards;

public sealed record CreateBoardCommand(string Name);
public sealed record BoardDto(Guid Id, string Name, DateTimeOffset CreatedUtc);

public interface ICreateBoardHandler
{
    Task<Result<BoardDto>> HandleAsync(CreateBoardCommand command, CancellationToken ct = default);
}

public sealed class CreateBoardHandler(IBoardRepository repository, IClock clock) : ICreateBoardHandler
{
    public async Task<Result<BoardDto>> HandleAsync(CreateBoardCommand command, CancellationToken ct = default)
    {
        // Validate uniqueness first (cheap check) then create domain object so we don't leak created IDs when failing.
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
