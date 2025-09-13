using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Non-negative dense order index within a parent scope.
/// </summary>
public readonly record struct OrderIndex(int Value)
{
    public static Result<OrderIndex> Create(int value)
    {
        if (value < 0)
            return Error.Validation("OrderIndex.Negative", "Order index cannot be negative");
        return new OrderIndex(value);
    }
    public override string ToString() => Value.ToString();
    public static implicit operator int(OrderIndex index) => index.Value;
}
