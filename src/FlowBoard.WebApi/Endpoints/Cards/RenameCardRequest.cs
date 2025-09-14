namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class RenameCardRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid CardId { get; set; }
    public string Title { get; set; } = string.Empty;
}
