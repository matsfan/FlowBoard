using FlowBoard.Application.UseCases.Boards.List;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class ListBoardsHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly ListBoardsHandler _handler;

    public ListBoardsHandlerTests()
    {
        _handler = new ListBoardsHandler(_repo);
    }

    [Fact]
    public async Task Returns_Boards()
    {
        var clock = new SystemClock();
        var testUserId = new UserId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
        var b1 = Board.Create("One", testUserId, clock).Value!;
        var b2 = Board.Create("Two", testUserId, clock).Value!;
        _repo.ListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IReadOnlyCollection<Board>>(new[] { b1, b2 }));

        var result = await _handler.HandleAsync(new ListBoardsQuery());
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value!, b => b.Name == "One");
        Assert.Contains(result.Value!, b => b.Name == "Two");
    }
}
