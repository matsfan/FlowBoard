namespace FlowBoard.Application.UseCases.Boards;

public sealed record BoardDto(Guid Id, string Name, DateTimeOffset CreatedUtc);
