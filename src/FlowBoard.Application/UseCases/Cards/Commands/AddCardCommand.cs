namespace FlowBoard.Application.UseCases.Cards.Commands;

public sealed record AddCardCommand(Guid BoardId, Guid ColumnId, string Title, string? Description);
