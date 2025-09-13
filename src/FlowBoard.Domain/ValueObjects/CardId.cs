namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a Card entity.
/// </summary>
public readonly record struct CardId(Guid Value)
{
    public static CardId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(CardId id) => id.Value;
    public static implicit operator CardId(Guid value) => new(value);
}
