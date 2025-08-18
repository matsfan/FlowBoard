using FlowBoard.Domain;
using FlowBoard.Infrastructure.Boards;
using FlowBoard.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Tests;

public class EfBoardRepositoryTests
{
    private static FlowBoardDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FlowBoardDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new FlowBoardDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Add_And_Retrieve_Board()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var ctx = CreateContext(connection);
        var repo = new EfBoardRepository(ctx);
        var clock = new SystemClock();

        var created = Board.Create("Infra EF Board", clock);
        Assert.True(created.IsSuccess);
        await repo.AddAsync(created.Value!);

        var loaded = await repo.GetByIdAsync(created.Value!.Id);
        Assert.NotNull(loaded);
        Assert.Equal(created.Value!.Name, loaded!.Name);
    }
}
