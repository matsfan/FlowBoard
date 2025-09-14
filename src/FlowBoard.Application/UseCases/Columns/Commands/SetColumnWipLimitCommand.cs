namespace FlowBoard.Application.UseCases.Columns.Commands;
public sealed record SetColumnWipLimitCommand(Guid BoardId, Guid ColumnId, int? WipLimit);
