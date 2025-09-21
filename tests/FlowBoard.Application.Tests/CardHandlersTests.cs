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
    private static readonly UserId TestUserId = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    private Board CreateBoardWithColumnAndCard(out ColumnId columnId, out CardId cardId)
    {
        var board = Board.Create("Board", TestUserId, _clock).Value!;
        var col = board.AddColumn("Todo", TestUserId, null).Value!;
        var card = board.AddCard(col.Id, "Card 1", null, TestUserId, _clock).Value!;
        columnId = col.Id; cardId = card.Id; return board;
    }
}
