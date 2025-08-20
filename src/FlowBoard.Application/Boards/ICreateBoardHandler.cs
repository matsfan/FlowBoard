using FlowBoard.Domain;

namespace FlowBoard.Application.Boards;

public interface ICreateBoardHandler
{
    Task<Result<BoardDto>> HandleAsync(CreateBoardCommand command, CancellationToken ct = default);
}
