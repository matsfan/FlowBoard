
namespace FlowBoard.WebApi.Endpoints.Cards.Update;

public sealed class UpdateCardRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ColumnId { get; set; } // Final column id (may differ from route column)
    public int Order { get; set; }
    public bool IsArchived { get; set; }
}