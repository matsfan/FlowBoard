
namespace FlowBoard.WebApi.Endpoints.Cards.Get;

public sealed class GetCardResponse { public Guid Id { get; set; } public Guid BoardId { get; set; } public Guid ColumnId { get; set; } public string Title { get; set; } = string.Empty; public string? Description { get; set; } = string.Empty; public int Order { get; set; } public bool IsArchived { get; set; } public DateTimeOffset CreatedUtc { get; set; } }
