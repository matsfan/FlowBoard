using FlowBoard.Domain;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlowBoard.Infrastructure.Persistence.Ef;

public sealed class FlowBoardDbContext(DbContextOptions<FlowBoardDbContext> options) : DbContext(options)
{
    public DbSet<Board> Boards => Set<Board>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var boardIdConverter = new ValueConverter<BoardId, Guid>(
            id => id.Value,
            value => new BoardId(value));

        modelBuilder.Entity<Board>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasConversion(boardIdConverter)
                .ValueGeneratedNever();

            b.OwnsOne(x => x.Name, nb =>
            {
                nb.Property(n => n.Value)
                    .HasColumnName("Name")
                    .IsRequired()
                    .HasMaxLength(100);
                nb.HasIndex(n => n.Value).IsUnique();
            });

            b.Property(x => x.CreatedUtc).IsRequired();
        });
    }
}
