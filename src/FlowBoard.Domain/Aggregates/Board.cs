using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Aggregates;

public sealed class Board
{
    // EF Core parameterless constructor
    private Board() { }

    private Board(BoardId id, BoardName name, DateTimeOffset createdUtc)
    {
        Id = id;
        Name = name;
        CreatedUtc = createdUtc;
    }

    public BoardId Id { get; }
    public BoardName Name { get; private set; } = null!;
    public DateTimeOffset CreatedUtc { get; }

    public static Result<Board> Create(string name, IClock clock)
    {
        var nameResult = BoardName.Create(name);
        if (nameResult.IsFailure)
            return Result<Board>.Failure(nameResult.Errors);

        var now = clock.UtcNow;
        var board = new Board(BoardId.New(), nameResult.Value!, now);
        return board;
    }

    public Result Rename(string newName)
    {
        var nameResult = BoardName.Create(newName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Errors);

        // no-op guard
        if (Name.Value == nameResult.Value!.Value)
            return Result.Success();

        Name = nameResult.Value!;
        return Result.Success();
    }
}

