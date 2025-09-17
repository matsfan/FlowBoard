namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class MoveCardRequest
{
    public Guid BoardId { get; set; }
    public Guid CardId { get; set; }
    public Guid FromColumnId { get; set; }
    public Guid ToColumnId { get; set; }
    public int TargetOrder { get; set; }
}
