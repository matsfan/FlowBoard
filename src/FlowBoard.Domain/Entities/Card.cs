using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Entities;

public sealed class Card
{
    // EF
    private Card() { }
    internal Card(CardId id, CardTitle title, CardDescription description, OrderIndex order, DateTimeOffset createdUtc)
    {
        Id = id;
        Title = title;
        Description = description;
        Order = order;
        CreatedUtc = createdUtc;
    }

    public CardId Id { get; private set; }
    public CardTitle Title { get; private set; } = null!;
    public CardDescription Description { get; private set; } = null!;
    public OrderIndex Order { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public bool IsArchived { get; private set; }

    internal void Rename(CardTitle newTitle) => Title = newTitle;
    internal void ChangeDescription(CardDescription newDescription) => Description = newDescription;
    internal void SetOrder(OrderIndex order) => Order = order;
    internal void Archive() => IsArchived = true;
    internal void Restore() => IsArchived = false;
}
