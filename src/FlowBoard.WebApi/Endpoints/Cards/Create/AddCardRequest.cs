namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class AddCardRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
