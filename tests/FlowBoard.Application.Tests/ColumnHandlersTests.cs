using FlowBoard.Application.UseCases.Columns.Commands;
using FlowBoard.Application.UseCases.Columns.Handlers;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace FlowBoard.Application.Tests;

public class ColumnHandlersTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();

    private Board CreateBoard()
    {
        return Board.Create("Board", _clock).Value!;
    }

    [Fact]
    public async Task AddColumn_Succeeds()
    {
        var board = CreateBoard();
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new AddColumnHandler(_repo);

        var result = await handler.HandleAsync(new AddColumnCommand(board.Id.Value, "Todo", null));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddColumn_Fails_When_Board_NotFound()
    {
        var handler = new AddColumnHandler(_repo);
        var result = await handler.HandleAsync(new AddColumnCommand(Guid.NewGuid(), "Todo", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.NotFound");
    }

    [Fact]
    public async Task RenameColumn_Succeeds()
    {
        var board = CreateBoard();
        var col = board.AddColumn("Todo", null).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new RenameColumnHandler(_repo);

        var result = await handler.HandleAsync(new RenameColumnCommand(board.Id.Value, col.Id.Value, "Next"));
        Assert.True(result.IsSuccess);
        Assert.Equal("Next", col.Name.Value);
    }

    [Fact]
    public async Task RenameColumn_Fails_Column_NotFound()
    {
        var board = CreateBoard();
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new RenameColumnHandler(_repo);
        var result = await handler.HandleAsync(new RenameColumnCommand(board.Id.Value, Guid.NewGuid(), "Next"));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }

    [Fact]
    public async Task ReorderColumn_Succeeds()
    {
        var board = CreateBoard();
        var first = board.AddColumn("A", null).Value!;
        var second = board.AddColumn("B", null).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ReorderColumnHandler(_repo);

        var result = await handler.HandleAsync(new ReorderColumnCommand(board.Id.Value, second.Id.Value, 0));
        Assert.True(result.IsSuccess);
        // Ensure second column now has order 0
        Assert.Equal(0, board.Columns.First(c => c.Id == second.Id).Order.Value);
    }

    [Fact]
    public async Task SetWipLimit_Succeeds()
    {
        var board = CreateBoard();
        var col = board.AddColumn("Todo", null).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new SetColumnWipLimitHandler(_repo);
        var result = await handler.HandleAsync(new SetColumnWipLimitCommand(board.Id.Value, col.Id.Value, 3));
        Assert.True(result.IsSuccess);
        Assert.True(col.WipLimit.HasValue && col.WipLimit.Value.Value == 3);
    }

    [Fact]
    public async Task SetWipLimit_Fails_Invalid()
    {
        var board = CreateBoard();
        var col = board.AddColumn("Todo", null).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new SetColumnWipLimitHandler(_repo);
        var result = await handler.HandleAsync(new SetColumnWipLimitCommand(board.Id.Value, col.Id.Value, -1));
        Assert.True(result.IsFailure);
        var err = Assert.Single(result.Errors);
        Assert.Equal("Column.WipLimit.Invalid", err.Code);
    }
}
