namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class AddColumnRequest
{
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? WipLimit { get; set; }
}
