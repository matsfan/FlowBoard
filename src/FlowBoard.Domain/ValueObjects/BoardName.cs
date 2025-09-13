using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating validation and normalization rules for a board name.
/// </summary>
public sealed class BoardName
{
    private BoardName(string value) => Value = value;

    public string Value { get; }

    public override string ToString() => Value;

    public static Result<BoardName> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Error.Validation("Board.Name.Empty", "Name must be provided");
        var trimmed = input.Trim();
        if (trimmed.Length > 100)
            return Error.Validation("Board.Name.TooLong", "Name must be 100 characters or fewer");
        return new BoardName(trimmed);
    }
}
