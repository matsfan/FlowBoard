using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Tests;

public class CardTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock { public DateTimeOffset UtcNow { get; } = now; }

    [Fact]
    public void AddCard_To_Column_Assigns_Order_And_Timestamps()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new TestClock(now);
        var board = Board.Create("Board A", clock).Value!;
        var column = board.AddColumn("Todo").Value!;
        var cardResult = board.AddCard(column.Id, "Task 1", null, clock);
        Assert.True(cardResult.IsSuccess);
        var card = cardResult.Value!;
        Assert.Equal(0, card.Order.Value);
        Assert.Equal(now, card.CreatedUtc);
    }

    [Fact]
    public void AddCard_Respects_Wip_Limit()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new TestClock(now);
        var board = Board.Create("Board A", clock).Value!;
        var column = board.AddColumn("Todo", wipLimit: 1).Value!;
        var first = board.AddCard(column.Id, "Task 1", null, clock);
        Assert.True(first.IsSuccess);
        var second = board.AddCard(column.Id, "Task 2", null, clock);
        Assert.True(second.IsFailure);
        Assert.Contains(second.Errors, e => e.Code == "Column.WipLimit.Violation");
    }

    [Fact]
    public void AddCard_Fails_When_Column_NotFound()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var bogusId = new ColumnId(Guid.NewGuid());
        var result = board.AddCard(bogusId, "X", null, clock);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }
}
