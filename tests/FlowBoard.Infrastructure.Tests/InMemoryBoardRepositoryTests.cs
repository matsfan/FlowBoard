using FlowBoard.Domain.Aggregates;
using FlowBoard.Infrastructure.Persistence.InMemory;
using FlowBoard.Domain;

namespace FlowBoard.Infrastructure.Tests;

public class InMemoryBoardRepositoryTests
{
    [Fact]
    public async Task Add_And_Get()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var board = Board.Create("Board", clock).Value!;
    await repository.AddAsync(board);
    var loaded = await repository.GetByIdAsync(board.Id);
        Assert.NotNull(loaded);
    }

    [Fact]
    public async Task ExistsByNameAsync_CaseInsensitive()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var board = Board.Create("Board", clock).Value!;
    await repository.AddAsync(board);
    Assert.True(await repository.ExistsByNameAsync("board"));
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
        var board = Board.Create("Board", clock).Value!;
    await repository.AddAsync(board);
        board.Rename("Renamed");
    await repository.UpdateAsync(board);
    var loaded = await repository.GetByIdAsync(board.Id);
        Assert.Equal("Renamed", loaded!.Name.Value);
    }

    [Fact]
    public async Task ListAsync_Returns_All()
    {
    var repository = new InMemoryBoardRepository();
        var clock = new SystemClock();
    await repository.AddAsync(Board.Create("A", clock).Value!);
    await repository.AddAsync(Board.Create("B", clock).Value!);
    var list = await repository.ListAsync();
        Assert.Equal(2, list.Count);
    }
}
