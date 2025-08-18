using FlowBoard.Domain;

namespace FlowBoard.Domain.Tests;

public class BoardTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    [Fact]
    public void Create_Fails_When_Name_Empty()
    {
        var clock = new TestClock(DateTimeOffset.UnixEpoch);
        var result = Board.Create("   ", clock);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Name.Empty");
    }

    [Fact]
    public void Create_Succeeds()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new TestClock(now);
        var result = Board.Create("My Board", clock);
        Assert.True(result.IsSuccess);
        Assert.Equal("My Board", result.Value!.Name);
        Assert.Equal(now, result.Value.CreatedUtc);
    }

    [Fact]
    public void Rename_Valid()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Name", clock).Value!;
        var rename = board.Rename("New Name");
        Assert.True(rename.IsSuccess);
        Assert.Equal("New Name", board.Name);
    }
}
