using FlowBoard.Application.Abstractions;
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
}
