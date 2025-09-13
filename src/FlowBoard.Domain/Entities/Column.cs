using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Entities;

public sealed class Column
{
    private readonly List<Card> _cards = [];

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
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

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
        if (result.Value.Value < _cards.Count)
            return Error.Validation("Column.WipLimit.Violation", "Cannot set WIP limit below current card count");
        WipLimit = result.Value;
        return Result.Success();
    }

    internal Result<Card> AddCard(string title, string? description, DateTimeOffset createdUtc)
    {
        if (WipLimit.HasValue && _cards.Count >= WipLimit.Value.Value)
            return Error.Conflict("Column.WipLimit.Violation", "WIP limit reached for this column");

        var titleResult = CardTitle.Create(title);
        if (titleResult.IsFailure)
            return Result<Card>.Failure(titleResult.Errors);
        var descResult = CardDescription.Create(description);
        if (descResult.IsFailure)
            return Result<Card>.Failure(descResult.Errors);

        var order = OrderIndex.Create(_cards.Count).Value!;
        var card = new Card(CardId.New(), titleResult.Value!, descResult.Value!, order, createdUtc);
        _cards.Add(card);
        return card;
    }
}
