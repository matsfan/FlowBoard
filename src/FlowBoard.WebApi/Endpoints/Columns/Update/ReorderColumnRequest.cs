namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class ReorderColumnRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public int NewOrder { get; set; }
}
