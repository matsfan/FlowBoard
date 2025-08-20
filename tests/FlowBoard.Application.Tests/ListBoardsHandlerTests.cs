using FlowBoard.Application.Boards;
using FlowBoard.Domain;
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
        var b1 = Board.Create("One", clock).Value!;
        var b2 = Board.Create("Two", clock).Value!;
        _repo.ListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IReadOnlyCollection<Board>>(new[] { b1, b2 }));

        var result = await _handler.HandleAsync(new ListBoardsQuery());
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value!, b => b.Name == "One");
        Assert.Contains(result.Value!, b => b.Name == "Two");
    }
}
