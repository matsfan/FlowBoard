using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Column name value object (unique within a board, validation only handled here; uniqueness enforced by Board aggregate logic).
/// </summary>
public sealed class ColumnName
{
    private ColumnName(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;

    public static Result<ColumnName> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Error.Validation("Column.Name.Empty", "Name must be provided");
        var trimmed = input.Trim();
        if (trimmed.Length > 60)
            return Error.Validation("Column.Name.TooLong", "Name must be 60 characters or fewer");
        return new ColumnName(trimmed);
    }
}
