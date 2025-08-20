using FlowBoard.Domain;

namespace FlowBoard.Application.Boards;

public interface IListBoardsHandler
{
    Task<Result<IReadOnlyCollection<BoardDto>>> HandleAsync(ListBoardsQuery query, CancellationToken ct = default);
}
