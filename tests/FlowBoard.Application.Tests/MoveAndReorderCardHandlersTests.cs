using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class MoveAndReorderCardHandlersTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();

    private Board CreateBoardWithTwoColumnsAndCards(out ColumnId fromColumnId, out ColumnId toColumnId, out CardId movingCardId)
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var doingColumn = board.AddColumn("Doing").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        fromColumnId = todoColumn.Id; toColumnId = doingColumn.Id; movingCardId = card.Id; return board;
    }

    [Fact]
    public async Task MoveCard_Succeeds()
    {
    var board = CreateBoardWithTwoColumnsAndCards(out var fromColumnId, out var toColumnId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new MoveCardHandler(_repo);
    // MoveCardCommand signature: (BoardId, CardId, FromColumnId, ToColumnId, TargetOrder)
        var result = await handler.HandleAsync(new MoveCardCommand(board.Id.Value, cardId.Value, fromColumnId.Value, toColumnId.Value, 0));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCard_Fails_Target_Column_NotFound()
    {
    var board = CreateBoardWithTwoColumnsAndCards(out var fromColumnId, out var _, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new MoveCardHandler(_repo);
        var result = await handler.HandleAsync(new MoveCardCommand(board.Id.Value, cardId.Value, fromColumnId.Value, Guid.NewGuid(), 0));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
        await _repo.DidNotReceive().UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReorderCard_Succeeds()
    {
        var board = Board.Create("Board", _clock).Value!;
    var todoColumn = board.AddColumn("Todo").Value!;
    var firstCard = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
    var secondCard = board.AddCard(todoColumn.Id, "Card 2", null, _clock).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ReorderCardHandler(_repo);
    var result = await handler.HandleAsync(new ReorderCardCommand(board.Id.Value, todoColumn.Id.Value, secondCard.Id.Value, 0));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReorderCard_Fails_Invalid_Order()
    {
        var board = Board.Create("Board", _clock).Value!;
    var todoColumn = board.AddColumn("Todo").Value!;
    var onlyCard = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ReorderCardHandler(_repo);
    var result = await handler.HandleAsync(new ReorderCardCommand(board.Id.Value, todoColumn.Id.Value, onlyCard.Id.Value, 5));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.Move.InvalidOrder");
        await _repo.DidNotReceive().UpdateAsync(board, Arg.Any<CancellationToken>());
    }
}
