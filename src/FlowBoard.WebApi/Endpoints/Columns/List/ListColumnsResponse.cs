
namespace FlowBoard.WebApi.Endpoints.Columns.List;

public sealed class ListColumnsResponse
{
    public List<ColumnItem> Columns { get; set; } = [];
    public sealed class ColumnItem { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }
}
