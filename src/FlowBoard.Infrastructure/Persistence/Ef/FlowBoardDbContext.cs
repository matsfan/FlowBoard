using FlowBoard.Domain;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain.Entities;
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
        var columnIdConverter = new ValueConverter<ColumnId, Guid>(
            id => id.Value,
            value => new ColumnId(value));
        var orderIndexConverter = new ValueConverter<OrderIndex, int>(
            o => o.Value,
            v => new OrderIndex(v));

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

            b.OwnsMany(x => x.Columns, cb =>
            {
                cb.WithOwner().HasForeignKey("BoardId");
                cb.Property(c => c.Id)
                    .HasConversion(columnIdConverter)
                    .ValueGeneratedNever();
                cb.HasKey(c => c.Id);

                cb.OwnsOne(c => c.Name, nb =>
                {
                    nb.Property(n => n.Value)
                        .HasColumnName("Name")
                        .IsRequired()
                        .HasMaxLength(60);
                });

                cb.Property(c => c.Order)
                    .HasConversion(orderIndexConverter)
                    .IsRequired();

                cb.Property(c => c.WipLimit)
                    .HasConversion(
                        v => v.HasValue ? v.Value.Value : (int?)null,
                        v => v.HasValue ? new WipLimit(v.Value) : (WipLimit?)null)
                    .HasColumnName("WipLimit");

                cb.ToTable("Columns");
            });
        });
    }
}
