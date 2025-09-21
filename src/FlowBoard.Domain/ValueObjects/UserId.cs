namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a User.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid value) => new(value);
}