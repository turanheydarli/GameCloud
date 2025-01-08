using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class GameEntityConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasOne(g => g.Developer)
            .WithMany(d => d.Games)
            .HasForeignKey(g => g.DeveloperId);

        builder.HasOne(d => d.Image)
            .WithOne()
            .HasForeignKey<Game>(d => d.ImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(p => p.Id);
    }
}