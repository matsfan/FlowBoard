namespace FlowBoard.Application.UseCases.Cards;

public sealed record CardDto(Guid Id, string Title, string? Description, int Order, bool IsArchived, DateTimeOffset CreatedUtc);
