namespace FlowBoard.Application.UseCases.Columns.Commands;
public sealed record RenameColumnCommand(Guid BoardId, Guid ColumnId, string Name);
