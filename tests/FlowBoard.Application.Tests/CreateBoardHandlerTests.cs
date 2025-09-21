using FlowBoard.Application.UseCases.Boards.Create;
using FlowBoard.Domain;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class CreateBoardHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly CreateBoardHandler _handler;
    private static readonly UserId TestUserId = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    public CreateBoardHandlerTests()
    {
        _handler = new CreateBoardHandler(_repo, _clock, _userContext);
        _clock.UtcNow.Returns(DateTimeOffset.UnixEpoch);
        _userContext.CurrentUserId.Returns(TestUserId);
    }

    [Fact]
    public async Task Fails_On_Duplicate_Name()
    {
        _repo.ExistsByNameAsync("Board", Arg.Any<CancellationToken>()).Returns(true);
        var result = await _handler.HandleAsync(new CreateBoardCommand("Board"));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Name.Duplicate");
    }

    [Fact]
    public async Task Creates_Board()
    {
        _repo.ExistsByNameAsync("Board", Arg.Any<CancellationToken>()).Returns(false);
        var result = await _handler.HandleAsync(new CreateBoardCommand("Board"));
        Assert.True(result.IsSuccess);
        await _repo.Received(1).AddAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }
}
