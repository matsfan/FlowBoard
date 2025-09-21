using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.UseCases.Columns.Update;
public sealed class UpdateColumnHandler(IBoardRepository repository, IUserContext userContext) : IRequestHandler<UpdateColumnCommand, Result<ColumnDto>>
{
    public async Task<Result<ColumnDto>> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);
        if (board is null)
            return Error.NotFound("Board.NotFound", "Board not found");
        // Compute operations required to reach desired state.
        var columnId = new ColumnId(request.ColumnId);
        // Rename first (domain ensures uniqueness)
        var renameResult = board.RenameColumn(columnId, request.Name, userContext.CurrentUserId);
        if (renameResult.IsFailure)
            return Result<ColumnDto>.Failure(renameResult.Errors);
        // Reorder if needed
        var column = board.Columns.FirstOrDefault(c => c.Id.Value == request.ColumnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        if (column.Order.Value != request.Order)
        {
            var reorderResult = board.ReorderColumn(columnId, request.Order);
            if (reorderResult.IsFailure)
                return Result<ColumnDto>.Failure(reorderResult.Errors);
        }
        // Wip limit
        var wipResult = board.SetColumnWipLimit(columnId, request.WipLimit);
        if (wipResult.IsFailure)
            return Result<ColumnDto>.Failure(wipResult.Errors);
        await repository.UpdateAsync(board, cancellationToken);
        column = board.Columns.First(c => c.Id.Value == request.ColumnId);
        return new ColumnDto(column.Id.Value, column.Name.Value, column.Order.Value, column.WipLimit?.Value);
    }
}
