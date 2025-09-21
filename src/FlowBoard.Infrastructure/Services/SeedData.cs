
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain;
using FlowBoard.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Infrastructure.Services;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider sp)
    {
        var repo = sp.GetRequiredService<IBoardRepository>();
        var clock = sp.GetRequiredService<IClock>();
        var boards = await repo.ListAsync();
        if (boards.Count > 0) return; // already seeded

        // Use fixed user ID for seeding
        var seedUserId = new UserId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

        // Board 1
        var b1 = Board.Create("My Board", seedUserId, clock).Value!;
        var col1 = b1.AddColumn("To Do", seedUserId, 5).Value!;
        var col2 = b1.AddColumn("In Process", seedUserId, 3).Value!;
        var col3 = b1.AddColumn("Done", seedUserId).Value!;
        col1.AddCard("Task A", "First task", clock.UtcNow);
        col1.AddCard("Task B", null, clock.UtcNow);
        col2.AddCard("Task C", "In progress", clock.UtcNow);
        col3.AddCard("Task D", "Completed", clock.UtcNow);
        await repo.AddAsync(b1);

        // Board 2
        var b2 = Board.Create("Personal Board", seedUserId, clock).Value!;
        var colA = b2.AddColumn("Ideas", seedUserId).Value!;
        var colB = b2.AddColumn("In Progress", seedUserId).Value!;
        var colC = b2.AddColumn("Done", seedUserId).Value!;
        colA.AddCard("Read a book", null, clock.UtcNow);
        colB.AddCard("Write blog post", "Drafting...", clock.UtcNow);
        await repo.AddAsync(b2);
    }
}
