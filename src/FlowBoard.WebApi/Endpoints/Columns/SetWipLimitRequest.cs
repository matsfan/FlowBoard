namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class SetWipLimitRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public int? WipLimit { get; set; }
}
