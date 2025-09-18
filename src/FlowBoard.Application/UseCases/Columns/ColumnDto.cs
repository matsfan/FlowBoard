namespace FlowBoard.Application.UseCases.Columns;

public sealed record ColumnDto(Guid Id, string Name, int Order, int? WipLimit);
