
namespace FlowBoard.WebApi.Endpoints.Boards.List;

public sealed class ListBoardsResponse
{
    public IReadOnlyCollection<BoardItem> Boards { get; set; } = Array.Empty<BoardItem>();

    public sealed class BoardItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
