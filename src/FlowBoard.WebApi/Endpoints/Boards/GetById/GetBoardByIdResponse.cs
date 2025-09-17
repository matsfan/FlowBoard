namespace FlowBoard.WebApi.Endpoints.Boards.GetById;

public sealed class GetBoardByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}
