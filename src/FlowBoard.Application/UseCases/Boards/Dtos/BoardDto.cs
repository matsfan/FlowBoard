namespace FlowBoard.Application.UseCases.Boards.Dtos;

public sealed record BoardDto(Guid Id, string Name, DateTimeOffset CreatedUtc);
