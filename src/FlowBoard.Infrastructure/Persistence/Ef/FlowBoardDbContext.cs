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
        var cardIdConverter = new ValueConverter<CardId, Guid>(
            id => id.Value,
            value => new CardId(value));
        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));
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

                cb.OwnsMany(c => c.Cards, cardb =>
                {
                    // Each Card row references its owning Column via ColumnId
                    cardb.WithOwner().HasForeignKey("ColumnId");
                    cardb.Property(x => x.Id)
                        .HasConversion(cardIdConverter)
                        .ValueGeneratedNever();
                    cardb.HasKey(x => x.Id);
                    cardb.OwnsOne(x => x.Title, tb =>
                    {
                        tb.Property(t => t.Value)
                            .HasColumnName("Title")
                            .IsRequired()
                            .HasMaxLength(200);
                    });
                    cardb.OwnsOne(x => x.Description, db =>
                    {
                        db.Property(d => d.Value)
                            .HasColumnName("Description")
                            .HasMaxLength(5000);
                    });
                    cardb.Property(x => x.Order)
                        .HasConversion(orderIndexConverter)
                        .IsRequired();
                    cardb.Property(x => x.CreatedUtc).IsRequired();
                    cardb.Property(x => x.IsArchived).IsRequired();
                    cardb.ToTable("Cards");
                });

                cb.ToTable("Columns");
            });

            // BoardMembers mapping
            b.OwnsMany(x => x.Members, mb =>
            {
                // Each BoardMember row references its owning Board via BoardId
                mb.WithOwner().HasForeignKey("BoardId");
                
                // Configure the UserId property
                mb.Property(m => m.Id)
                    .HasConversion(userIdConverter)
                    .HasColumnName("UserId")
                    .IsRequired();
                    
                mb.Property(m => m.Role)
                    .HasConversion<int>()
                    .IsRequired();
                    
                mb.Property(m => m.JoinedAt).IsRequired();
                
                // Use auto-generated ID for simplicity
                mb.Property<int>("InternalId");
                mb.HasKey("InternalId");
                
                // Ensure uniqueness of User per Board - use the correct property name
                mb.HasIndex("BoardId", "Id").IsUnique();
                
                mb.ToTable("BoardMembers");
            });
        });
    }
}
