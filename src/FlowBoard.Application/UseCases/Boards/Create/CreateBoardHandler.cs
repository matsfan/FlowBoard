using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Application.UseCases.Boards.Create;
public sealed class CreateBoardHandler(IBoardRepository repository, IClock clock) : IRequestHandler<CreateBoardCommand, Result<BoardDto>>
{
    public async Task<Result<BoardDto>> HandleAsync(CreateBoardCommand command, CancellationToken ct = default)
    {
        if (await repository.ExistsByNameAsync(command.Name.Trim(), ct))
            return Error.Conflict("Board.Name.Duplicate", "A board with that name already exists");
        var boardResult = Board.Create(command.Name, clock);
        if (boardResult.IsFailure)
            return boardResult.Errors.ToArray();
        var board = boardResult.Value!;
        await repository.AddAsync(board, ct);
        return new BoardDto(board.Id.Value, board.Name.Value, board.CreatedUtc);
    }
    // MediatR handler entrypoint
    public Task<Result<BoardDto>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
