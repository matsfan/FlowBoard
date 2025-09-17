using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain;

namespace FlowBoard.Domain.Tests;

public class BoardDomainNegativeTests
{
    private readonly SystemClock _clock = new();

    private Board CreateBoardWithTwoColumnsAndCard(out ColumnId todoColumnId, out ColumnId doingColumnId, out CardId cardId)
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var doingColumn = board.AddColumn("Doing").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        todoColumnId = todoColumn.Id; doingColumnId = doingColumn.Id; cardId = card.Id; return board;
    }

    [Fact]
    public void AddColumn_Fails_Duplicate_Name_CaseInsensitive()
    {
        var board = Board.Create("Board", _clock).Value!;
        var firstAddResult = board.AddColumn("Todo");
        Assert.True(firstAddResult.IsSuccess);
        var duplicateAddResult = board.AddColumn("todo");
        Assert.True(duplicateAddResult.IsFailure);
        Assert.Contains(duplicateAddResult.Errors, e => e.Code == "Column.Name.Duplicate");
    }

    [Fact]
    public void AddCard_Fails_WipLimit_Reached()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo", 1).Value!; // WIP limit 1
        var firstCardResult = board.AddCard(todoColumn.Id, "Card 1", null, _clock);
        Assert.True(firstCardResult.IsSuccess);
        var secondCardResult = board.AddCard(todoColumn.Id, "Card 2", null, _clock);
        Assert.True(secondCardResult.IsFailure);
        Assert.Contains(secondCardResult.Errors, e => e.Code == "Column.WipLimit.Violation");
    }

    [Fact]
    public void MoveCard_Fails_Source_Column_NotFound()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        var missingSourceColumnId = ColumnId.New();
        var result = board.MoveCard(card.Id, missingSourceColumnId, todoColumn.Id, 0);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }

    [Fact]
    public void MoveCard_Fails_Target_Column_NotFound()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        var missingTargetColumnId = ColumnId.New();
        var result = board.MoveCard(card.Id, todoColumn.Id, missingTargetColumnId, 0);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }

    [Fact]
    public void MoveCard_Fails_Card_Not_In_Source()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var doingColumn = board.AddColumn("Doing").Value!;
        var card = board.AddCard(doingColumn.Id, "Card 1", null, _clock).Value!;
        var result = board.MoveCard(card.Id, todoColumn.Id, doingColumn.Id, 0);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.NotFound");
    }

    [Fact]
    public void MoveCard_Fails_Target_Order_Out_Of_Range()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var doingColumn = board.AddColumn("Doing").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        var result = board.MoveCard(card.Id, todoColumn.Id, doingColumn.Id, 5); // out of range (empty target)
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.Move.InvalidOrder");
    }

    [Fact]
    public void MoveCard_Fails_WipLimit_Target()
    {
        var board = Board.Create("Board", _clock).Value!;
        var sourceColumn = board.AddColumn("Todo").Value!;
        var targetColumn = board.AddColumn("Doing", 1).Value!; // WIP limit 1
        var existingCard = board.AddCard(targetColumn.Id, "Existing", null, _clock).Value!;
        var movingCard = board.AddCard(sourceColumn.Id, "Card 1", null, _clock).Value!;
        var result = board.MoveCard(movingCard.Id, sourceColumn.Id, targetColumn.Id, 1);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.WipLimit.Violation");
    }

    [Fact]
    public void ReorderColumn_Fails_Out_Of_Range()
    {
        var board = Board.Create("Board", _clock).Value!;
        var firstColumn = board.AddColumn("A").Value!;
        var secondColumn = board.AddColumn("B").Value!;
        var result = board.ReorderColumn(firstColumn.Id, 5);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.Order.Invalid");
    }

    [Fact]
    public void RenameBoard_Fails_Invalid_Name()
    {
        var board = Board.Create("Board", _clock).Value!;
        var result = board.Rename("");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code.StartsWith("Board.Name."));
    }

    [Fact]
    public void RenameBoard_Idempotent_Same_Name_Succeeds_No_Errors()
    {
        var board = Board.Create("Board", _clock).Value!;
        var result = board.Rename("Board");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ArchiveCard_Idempotent()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        var firstArchiveResult = board.ArchiveCard(todoColumn.Id, card.Id);
        Assert.True(firstArchiveResult.IsSuccess);
        var secondArchiveResult = board.ArchiveCard(todoColumn.Id, card.Id); // idempotent
        Assert.True(secondArchiveResult.IsSuccess);
    }

    [Fact]
    public void SetColumnWipLimit_Fails_Below_Current_Count()
    {
        var board = Board.Create("Board", _clock).Value!;
        var todoColumn = board.AddColumn("Todo").Value!;
        var card = board.AddCard(todoColumn.Id, "Card 1", null, _clock).Value!;
        var result = board.SetColumnWipLimit(todoColumn.Id, 0); // invalid (must be > 0) - domain returns Column.WipLimit.Invalid
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.WipLimit.Invalid");
    }
}
