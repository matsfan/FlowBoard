namespace FlowBoard.Application.Boards;

public sealed record BoardDto(Guid Id, string Name, DateTimeOffset CreatedUtc);
