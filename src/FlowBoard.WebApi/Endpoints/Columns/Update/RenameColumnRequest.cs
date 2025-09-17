namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class RenameColumnRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public string Name { get; set; } = string.Empty;
}
