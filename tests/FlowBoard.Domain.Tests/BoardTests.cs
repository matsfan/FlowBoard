using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Tests;

public class BoardTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
    
    // Test user for domain tests
    private static readonly UserId TestUserId = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    [Fact]
    public void Create_Fails_When_Name_Empty()
    {
        var clock = new TestClock(DateTimeOffset.UnixEpoch);
        var result = Board.Create("   ", TestUserId, clock);
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
        Assert.Equal("My Board", result.Value!.Name.Value);
        Assert.Equal(now, result.Value.CreatedUtc);
    }

    [Fact]
    public void Rename_Valid()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Name", clock).Value!;
        var rename = board.Rename("New Name");
        Assert.True(rename.IsSuccess);
        Assert.Equal("New Name", board.Name.Value);
    }
}
