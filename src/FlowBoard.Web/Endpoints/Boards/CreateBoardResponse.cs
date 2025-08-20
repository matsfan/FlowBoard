namespace FlowBoard.Web.Endpoints.Boards;

public sealed class CreateBoardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}
