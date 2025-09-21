using FlowBoard.Application.Abstractions;
using FlowBoard.Application.UseCases.Boards.Delete;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class DeleteBoardHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly DeleteBoardHandler _handler;

    public DeleteBoardHandlerTests()
    {
        _handler = new DeleteBoardHandler(_repo);
    }

    [Fact]
    public async Task Returns_NotFound_When_Missing()
    {
        _repo.GetByIdAsync(Arg.Any<BoardId>(), Arg.Any<CancellationToken>()).Returns((Board?)null);
        var id = Guid.NewGuid();
        var result = await _handler.Handle(new DeleteBoardCommand(id), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.NotFound");
    }

    [Fact]
    public async Task Deletes_When_Exists()
    {
        // Create a real board via domain factory so it's a valid aggregate
        var testUserId = new UserId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
        var create = Board.Create("Board to delete", testUserId, new SystemClock());
        Assert.True(create.IsSuccess);
        var existing = create.Value!;

        // Arrange repository to return the existing board by id
        _repo.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.Handle(new DeleteBoardCommand(existing.Id.Value), CancellationToken.None);
        Assert.True(result.IsSuccess);
        await _repo.Received(1).DeleteAsync(existing, Arg.Any<CancellationToken>());
    }
}
