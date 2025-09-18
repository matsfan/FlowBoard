using FlowBoard.Application.UseCases.Boards.Create;
using FlowBoard.Domain;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Aggregates;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class CreateBoardHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CreateBoardHandler _handler;

    public CreateBoardHandlerTests()
    {
        _handler = new CreateBoardHandler(_repo, _clock);
        _clock.UtcNow.Returns(DateTimeOffset.UnixEpoch);
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
