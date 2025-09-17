namespace FlowBoard.Application.UseCases.Columns.Commands;

public sealed record AddColumnCommand(Guid BoardId, string Name, int? WipLimit);
