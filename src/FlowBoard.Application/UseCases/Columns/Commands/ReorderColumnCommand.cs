namespace FlowBoard.Application.UseCases.Columns.Commands;
public sealed record ReorderColumnCommand(Guid BoardId, Guid ColumnId, int NewOrder);
