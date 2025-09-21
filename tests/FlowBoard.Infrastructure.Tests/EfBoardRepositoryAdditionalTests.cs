using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Infrastructure.Persistence.Ef;
using FlowBoard.Infrastructure.Persistence.Ef.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FlowBoard.Domain;

namespace FlowBoard.Infrastructure.Tests;

public class EfBoardRepositoryAdditionalTests
{
    private static FlowBoardDbContext CreateContext(SqliteConnection sqlConnection)
    {
        var options = new DbContextOptionsBuilder<FlowBoardDbContext>()
            .UseSqlite(sqlConnection)
            .Options;
        var context = new FlowBoardDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task ExistsByNameAsync_Is_Case_Insensitive()
    {
        using var inMemorySqliteConnection = new SqliteConnection("DataSource=:memory:");
        await inMemorySqliteConnection.OpenAsync();
        await using var context = CreateContext(inMemorySqliteConnection);
        var repository = new EfBoardRepository(context);
        var clock = new SystemClock();
        var userId = UserId.New();
        var board = Board.Create("My Board", userId, clock).Value!;
        await repository.AddAsync(board);
        var exists = await repository.ExistsByNameAsync("my board");
        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes()
    {
        using var inMemorySqliteConnection = new SqliteConnection("DataSource=:memory:");
        await inMemorySqliteConnection.OpenAsync();
        await using var context = CreateContext(inMemorySqliteConnection);
        var repository = new EfBoardRepository(context);
        var clock = new SystemClock();
        var userId = UserId.New();
        var board = Board.Create("Board", userId, clock).Value!;
        await repository.AddAsync(board);
        board.Rename("Renamed", userId);
        await repository.UpdateAsync(board);
        var reloaded = await repository.GetByIdAsync(board.Id);
        Assert.Equal("Renamed", reloaded!.Name.Value);
    }
}
