using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;
using Xunit;
using FlowBoard.Application.UseCases.Columns.Create;

namespace FlowBoard.Application.Tests;

public class ColumnHandlersTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly SystemClock _clock = new();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private static readonly UserId TestUserId = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    public ColumnHandlersTests()
    {
        _userContext.CurrentUserId.Returns(TestUserId);
    }

    private Board CreateBoard()
    {
        return Board.Create("Board", TestUserId, _clock).Value!;
    }

    [Fact]
    public async Task AddColumn_Succeeds()
    {
        var board = CreateBoard();
        _repo.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        var handler = new CreateColumnHandler(_repo, _userContext);

        var result = await handler.HandleAsync(new CreateColumnCommand(board.Id.Value, "Todo", null));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).UpdateAsync(board, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddColumn_Fails_When_Board_NotFound()
    {
        var handler = new CreateColumnHandler(_repo, _userContext);
        var result = await handler.HandleAsync(new CreateColumnCommand(Guid.NewGuid(), "Todo", null));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.NotFound");
    }

}
