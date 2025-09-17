namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class ChangeDescriptionRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid CardId { get; set; }
    public string? Description { get; set; }
}
