namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ReorderCardRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid CardId { get; set; }
    public int NewOrder { get; set; }
}
