namespace FlowBoard.Application.UseCases.Columns.Dto;

public sealed record ColumnDto(Guid Id, string Name, int Order, int? WipLimit);
