using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Columns.Create;
public sealed class CreateColumnHandler(IBoardRepository repository, IUserContext userContext) : IRequestHandler<CreateColumnCommand, Result<ColumnDto>>
{
    public async Task<Result<ColumnDto>> HandleAsync(CreateColumnCommand command, CancellationToken ct = default)
    {
        var board = await repository.GetByIdAsync(new BoardId(command.BoardId), ct);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        var result = board.AddColumn(command.Name, userContext.CurrentUserId, command.WipLimit);
        if (result.IsFailure)
            return result.Errors.ToArray();
        await repository.UpdateAsync(board, ct);
        var column = result.Value!;
        return new ColumnDto(column.Id.Value, column.Name.Value, column.Order.Value, column.WipLimit?.Value);
    }
    public Task<Result<ColumnDto>> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
