using FastEndpoints;
using MediatR;

namespace FlowBoard.WebApi.Endpoints.Cards.List;

public sealed class ListCardsResponse
{
    public List<CardItem> Cards { get; set; } = [];
    public sealed class CardItem { public Guid Id { get; set; } public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public int Order { get; set; } public bool IsArchived { get; set; } public DateTimeOffset CreatedUtc { get; set; } }
}
