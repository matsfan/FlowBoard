using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Entities;

public sealed class Column
{
    // EF
    private Column() { }
    internal Column(ColumnId id, ColumnName name, OrderIndex order, WipLimit? wipLimit)
    {
        Id = id;
        Name = name;
        Order = order;
        WipLimit = wipLimit;
    }

    public ColumnId Id { get; private set; }
    public ColumnName Name { get; private set; } = null!;
    public OrderIndex Order { get; private set; }
    public WipLimit? WipLimit { get; private set; }

    internal void Rename(ColumnName newName) => Name = newName;
    internal void SetOrder(OrderIndex order) => Order = order;
    internal Result SetWipLimit(int? value)
    {
        if (value is null)
        {
            WipLimit = null;
            return Result.Success();
        }
        var result = ValueObjects.WipLimit.Create(value.Value);
        if (result.IsFailure)
            return Result.Failure(result.Errors);
        WipLimit = result.Value;
        return Result.Success();
    }
}
