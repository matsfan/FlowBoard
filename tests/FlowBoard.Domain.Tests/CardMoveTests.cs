using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Tests;

public class CardMoveTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock { public DateTimeOffset UtcNow { get; } = now; }

    [Fact]
    public void MoveCard_Between_Columns_Respects_Wip()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var from = board.AddColumn("Todo").Value!;
        var to = board.AddColumn("Doing", wipLimit:1).Value!;
        var card = board.AddCard(from.Id, "Task 1", null, clock).Value!;
        // First move should succeed
        var move1 = board.MoveCard(card.Id, from.Id, to.Id, 0);
        Assert.True(move1.IsSuccess);
        // Add another card to source and attempt move exceeding WIP
        var card2 = board.AddCard(from.Id, "Task 2", null, clock).Value!;
        var move2 = board.MoveCard(card2.Id, from.Id, to.Id, 1);
        Assert.True(move2.IsFailure);
        Assert.Contains(move2.Errors, e => e.Code == "Column.WipLimit.Violation");
    }

    [Fact]
    public void ReorderCardWithinColumn_Updates_Order()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var column = board.AddColumn("Todo").Value!;
        var c1 = board.AddCard(column.Id, "A", null, clock).Value!;
        var c2 = board.AddCard(column.Id, "B", null, clock).Value!;
        var c3 = board.AddCard(column.Id, "C", null, clock).Value!;

        var reorder = board.ReorderCardWithinColumn(column.Id, c3.Id, 0);
        Assert.True(reorder.IsSuccess);
        var ordered = column.Cards.OrderBy(c => c.Order.Value).Select(c => c.Id).ToList();
        Assert.Equal(new[] { c3.Id, c1.Id, c2.Id }, ordered);
    }
}
