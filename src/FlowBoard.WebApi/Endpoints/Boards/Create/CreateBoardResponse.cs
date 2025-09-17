namespace FlowBoard.WebApi.Endpoints.Boards.Create;

public sealed class CreateBoardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}
