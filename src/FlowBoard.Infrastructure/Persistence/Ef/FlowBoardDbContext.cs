using FlowBoard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Persistence.Ef;

public sealed class FlowBoardDbContext(DbContextOptions<FlowBoardDbContext> options) : DbContext(options)
{
    public DbSet<Board> Boards => Set<Board>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.CreatedUtc).IsRequired();
        });
    }
}
