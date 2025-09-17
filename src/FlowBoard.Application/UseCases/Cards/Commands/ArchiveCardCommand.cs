namespace FlowBoard.Application.UseCases.Cards.Commands;
public sealed record ArchiveCardCommand(Guid BoardId, Guid ColumnId, Guid CardId);
