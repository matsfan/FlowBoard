namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a Column entity.
/// </summary>
public readonly record struct ColumnId(Guid Value)
{
    public static ColumnId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ColumnId id) => id.Value;
    public static implicit operator ColumnId(Guid value) => new(value);
}
