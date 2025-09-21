using FlowBoard.Domain.Aggregates;
using FlowBoard.Infrastructure.Persistence.InMemory;
using FlowBoard.Domain;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Infrastructure.Tests;

public class InMemoryBoardRepositoryTests
{
    [Fact]
    public async Task Add_And_Get()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var userId = UserId.New();
        var board = Board.Create("Board", userId, clock).Value!;
    await repository.AddAsync(board);
    var loaded = await repository.GetByIdAsync(board.Id);
        Assert.NotNull(loaded);
    }

    [Fact]
    public async Task ExistsByNameAsync_CaseInsensitive()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var userId = UserId.New();
        var board = Board.Create("Board", userId, clock).Value!;
    await repository.AddAsync(board);
    Assert.True(await repository.ExistsByNameAsync("board"));
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var userId = UserId.New();
        var board = Board.Create("Board", userId, clock).Value!;
    await repository.AddAsync(board);
        board.Rename("Renamed", userId);
    await repository.UpdateAsync(board);
    var loaded = await repository.GetByIdAsync(board.Id);
        Assert.Equal("Renamed", loaded!.Name.Value);
    }

    [Fact]
    public async Task ListAsync_Returns_All()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var userId = UserId.New();
    await repository.AddAsync(Board.Create("A", userId, clock).Value!);
    await repository.AddAsync(Board.Create("B", userId, clock).Value!);
    var list = await repository.ListAsync();
        Assert.Equal(2, list.Count);
    }
}
