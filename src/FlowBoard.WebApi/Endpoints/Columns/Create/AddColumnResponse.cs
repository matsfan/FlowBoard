namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class AddColumnResponse
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? WipLimit { get; set; }
}
