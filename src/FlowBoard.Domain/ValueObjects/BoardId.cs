namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a Board aggregate.
/// </summary>
public readonly record struct BoardId(Guid Value)
{
    public static BoardId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(BoardId id) => id.Value;
    public static implicit operator BoardId(Guid value) => new(value);
}