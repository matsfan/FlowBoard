namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ArchiveCardRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid CardId { get; set; }
}
