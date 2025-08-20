using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.Aggregates;

public sealed class Board
{
    // EF Core parameterless constructor
    private Board() { }

    private Board(Guid id, string name, DateTimeOffset createdUtc)
    {
        Id = id;
        Name = name;
        CreatedUtc = createdUtc;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; }

    public static Result<Board> Create(string name, IClock clock)
    {
        var validation = ValidateName(name);
        if (validation.IsFailure)
            return Result<Board>.Failure(validation.Errors);

        var now = clock.UtcNow;
        var board = new Board(Guid.NewGuid(), name.Trim(), now);
        return board;
    }

    public Result Rename(string newName)
    {
        var validation = ValidateName(newName);
        if (validation.IsFailure)
            return Result.Failure(validation.Errors);
        Name = newName.Trim();
        return Result.Success();
    }

    private static Result ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Board.Name.Empty", "Name must be provided");
        name = name.Trim();
        if (name.Length > 100)
            return Error.Validation("Board.Name.TooLong", "Name must be 100 characters or fewer");
        return Result.Success();
    }
}

// Clock abstractions moved to separate files (IClock.cs, SystemClock.cs)
