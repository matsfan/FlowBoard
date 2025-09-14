using FlowBoard.Application.UseCases.Cards.Commands;
using FlowBoard.Application.UseCases.Cards.Handlers;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class CardHandlersTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();

    private Board CreateBoardWithColumnAndCard(out ColumnId columnId, out CardId cardId)
    {
        var board = Board.Create("Board", _clock).Value!;
        var col = board.AddColumn("Todo", null).Value!;
        var card = board.AddCard(col.Id, "Card 1", null, _clock).Value!;
        columnId = col.Id; cardId = card.Id; return board;
    }

    [Fact]
    public async Task ArchiveCard_Succeeds()
    {
        var board = CreateBoardWithColumnAndCard(out var colId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ArchiveCardHandler(_repo);
        var result = await handler.HandleAsync(new ArchiveCardCommand(board.Id.Value, colId.Value, cardId.Value));
        Assert.True(result.IsSuccess);
        Assert.True(board.Columns.First(c => c.Id == colId).Cards.First(c => c.Id == cardId).IsArchived);
    }

    [Fact]
    public async Task ArchiveCard_Fails_Card_NotFound()
    {
        var board = Board.Create("Board", _clock).Value!;
        var col = board.AddColumn("Todo", null).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ArchiveCardHandler(_repo);
        var result = await handler.HandleAsync(new ArchiveCardCommand(board.Id.Value, col.Id.Value, Guid.NewGuid()));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.NotFound");
    }

    [Fact]
    public async Task RenameCard_Succeeds()
    {
        var board = CreateBoardWithColumnAndCard(out var colId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new RenameCardHandler(_repo);
        var result = await handler.HandleAsync(new RenameCardCommand(board.Id.Value, colId.Value, cardId.Value, "New Title"));
        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", board.Columns.First(c => c.Id == colId).Cards.First(c => c.Id == cardId).Title.Value);
    }

    [Fact]
    public async Task RenameCard_Fails_Invalid_Title()
    {
        var board = CreateBoardWithColumnAndCard(out var colId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new RenameCardHandler(_repo);
        var result = await handler.HandleAsync(new RenameCardCommand(board.Id.Value, colId.Value, cardId.Value, ""));
        Assert.True(result.IsFailure);
    var err = Assert.Single(result.Errors);
    Assert.Equal("Card.Title.Empty", err.Code);
    }

    [Fact]
    public async Task ChangeDescription_Succeeds()
    {
        var board = CreateBoardWithColumnAndCard(out var colId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new ChangeCardDescriptionHandler(_repo);
        var result = await handler.HandleAsync(new ChangeCardDescriptionCommand(board.Id.Value, colId.Value, cardId.Value, "Desc"));
        Assert.True(result.IsSuccess);
        Assert.Equal("Desc", board.Columns.First(c => c.Id == colId).Cards.First(c => c.Id == cardId).Description.Value);
    }

    [Fact]
    public async Task DeleteCard_Succeeds()
    {
        var board = CreateBoardWithColumnAndCard(out var colId, out var cardId);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new DeleteCardHandler(_repo);
        var result = await handler.HandleAsync(new DeleteCardCommand(board.Id.Value, colId.Value, cardId.Value));
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(board.Columns.First(c => c.Id == colId).Cards, c => c.Id == cardId);
    }
}
