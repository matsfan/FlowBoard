using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Work-in-progress limit (nullable semantics handled by caller; this represents a concrete positive limit).
/// </summary>
public readonly record struct WipLimit(int Value)
{
    public static Result<WipLimit> Create(int value)
    {
        if (value <= 0)
            return Error.Validation("Column.WipLimit.Invalid", "WIP limit must be greater than zero");
        return new WipLimit(value);
    }
    public override string ToString() => Value.ToString();
}
