using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class AddCardHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();

    [Fact]
    public async Task AddCard_Succeeds()
    {
        var board = Board.Create("Board", _clock).Value!;
    var todoColumn = board.AddColumn("Todo").Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new AddCardHandler(_repo, _clock);
    var result = await handler.HandleAsync(new AddCardCommand(board.Id.Value, todoColumn.Id.Value, "Card 1", null));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCard_Fails_Column_NotFound()
    {
        var board = Board.Create("Board", _clock).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new AddCardHandler(_repo, _clock);
        var result = await handler.HandleAsync(new AddCardCommand(board.Id.Value, Guid.NewGuid(), "Card 1", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }

    [Fact]
    public async Task AddCard_Fails_Invalid_Title()
    {
        var board = Board.Create("Board", _clock).Value!;
    var todoColumn = board.AddColumn("Todo").Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new AddCardHandler(_repo, _clock);
    var result = await handler.HandleAsync(new AddCardCommand(board.Id.Value, todoColumn.Id.Value, "", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.Title.Empty");
    }

    [Fact]
    public async Task AddCard_Fails_WipLimit()
    {
        var board = Board.Create("Board", _clock).Value!;
    var limitedTodoColumn = board.AddColumn("Todo", 1).Value!;
    var firstCardResult = board.AddCard(limitedTodoColumn.Id, "Card 1", null, _clock);
    Assert.True(firstCardResult.IsSuccess);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new AddCardHandler(_repo, _clock);
    var result = await handler.HandleAsync(new AddCardCommand(board.Id.Value, limitedTodoColumn.Id.Value, "Card 2", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.WipLimit.Violation");
    }
}
