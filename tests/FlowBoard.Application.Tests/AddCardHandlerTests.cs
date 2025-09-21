using FlowBoard.Application.UseCases.Cards.Create;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class AddCardHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private static readonly UserId TestUserId = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    public AddCardHandlerTests()
    {
        _userContext.CurrentUserId.Returns(TestUserId);
    }

    [Fact]
    public async Task AddCard_Succeeds()
    {
        var board = Board.Create("Board", TestUserId, _clock).Value!;
        var todoColumn = board.AddColumn("Todo", TestUserId).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new CreateCardHandler(_repo, _clock, _userContext);
        var result = await handler.HandleAsync(new CreateCardCommand(board.Id.Value, todoColumn.Id.Value, "Card 1", null));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCard_Fails_Column_NotFound()
    {
        var board = Board.Create("Board", TestUserId, _clock).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new CreateCardHandler(_repo, _clock, _userContext);
        var result = await handler.HandleAsync(new CreateCardCommand(board.Id.Value, Guid.NewGuid(), "Card 1", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.NotFound");
    }

    [Fact]
    public async Task AddCard_Fails_Invalid_Title()
    {
        var board = Board.Create("Board", TestUserId, _clock).Value!;
        var todoColumn = board.AddColumn("Todo", TestUserId).Value!;
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new CreateCardHandler(_repo, _clock, _userContext);
        var result = await handler.HandleAsync(new CreateCardCommand(board.Id.Value, todoColumn.Id.Value, "", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Card.Title.Empty");
    }

    [Fact]
    public async Task AddCard_Fails_WipLimit()
    {
        var board = Board.Create("Board", TestUserId, _clock).Value!;
        var limitedTodoColumn = board.AddColumn("Todo", TestUserId, 1).Value!;
        var firstCardResult = board.AddCard(limitedTodoColumn.Id, "Card 1", null, TestUserId, _clock);
        Assert.True(firstCardResult.IsSuccess);
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new CreateCardHandler(_repo, _clock, _userContext);
        var result = await handler.HandleAsync(new CreateCardCommand(board.Id.Value, limitedTodoColumn.Id.Value, "Card 2", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Column.WipLimit.Violation");
    }
}
