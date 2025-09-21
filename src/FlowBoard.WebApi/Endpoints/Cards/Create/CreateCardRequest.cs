namespace FlowBoard.WebApi.Endpoints.Cards.Create;

public sealed class CreateCardRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
}
