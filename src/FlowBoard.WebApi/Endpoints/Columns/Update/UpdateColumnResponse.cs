
namespace FlowBoard.WebApi.Endpoints.Columns.Update;

public sealed class UpdateColumnResponse { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }
