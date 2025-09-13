using FlowBoard.Domain;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Infrastructure.Persistence.Ef;
using FlowBoard.Infrastructure.Persistence.Ef.Repositories;
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
        Assert.Equal(created.Value!.Name.Value, loaded!.Name.Value);
    }

    [Fact]
    public async Task List_Returns_All_In_Order()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var ctx = CreateContext(connection);
        var repo = new EfBoardRepository(ctx);
        var clock = new SystemClock();

        var b1 = Board.Create("Alpha", clock).Value!;
        await repo.AddAsync(b1);
        var b2 = Board.Create("Beta", clock).Value!;
        await repo.AddAsync(b2);

        var list = await repo.ListAsync();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, b => b.Name.Value == "Alpha");
        Assert.Contains(list, b => b.Name.Value == "Beta");
    }
}
