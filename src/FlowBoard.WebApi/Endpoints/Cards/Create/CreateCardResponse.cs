namespace FlowBoard.WebApi.Endpoints.Cards.Create;

public sealed class CreateCardResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
