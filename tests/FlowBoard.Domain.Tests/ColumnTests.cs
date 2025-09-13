using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;

namespace FlowBoard.Domain.Tests;

public class ColumnTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock { public DateTimeOffset UtcNow { get; } = now; }

    [Fact]
    public void Add_First_Column_Assigns_Order_Zero()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var colResult = board.AddColumn("Todo");
        Assert.True(colResult.IsSuccess);
        Assert.Equal(0, colResult.Value!.Order.Value);
    }

    [Fact]
    public void AddColumn_Duplicate_Name_Fails()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        Assert.True(board.AddColumn("Todo").IsSuccess);
        var dup = board.AddColumn("todo"); // case-insensitive
        Assert.True(dup.IsFailure);
        Assert.Contains(dup.Errors, e => e.Code == "Column.Name.Duplicate");
    }

    [Fact]
    public void RenameColumn_Succeeds()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var col = board.AddColumn("Todo").Value!;
        var rename = board.RenameColumn(col.Id, "In Progress");
        Assert.True(rename.IsSuccess);
        Assert.Equal("In Progress", board.Columns.Single().Name.Value);
    }

    [Fact]
    public void ReorderColumn_Changes_Order_And_Normalizes()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Board A", clock).Value!;
        var c1 = board.AddColumn("Todo").Value!;
        var c2 = board.AddColumn("Doing").Value!;
        var c3 = board.AddColumn("Done").Value!;

        var reorder = board.ReorderColumn(c3.Id, 0);
        Assert.True(reorder.IsSuccess);
        var ordered = board.Columns.OrderBy(c => c.Order.Value).ToList();
        Assert.Equal(new[] { c3.Id, c1.Id, c2.Id }, ordered.Select(c => c.Id));
        Assert.Equal(new[] {0,1,2}, ordered.Select(c => c.Order.Value));
    }
}
