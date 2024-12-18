using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class GameKeyEntityConfiguration : IEntityTypeConfiguration<GameKey>
{
    public void Configure(EntityTypeBuilder<GameKey> builder)
    {
        builder.HasOne(g => g.Game)
            .WithMany(d => d.GameKeys)
            .HasForeignKey(g => g.GameId);
        
        builder.HasKey(p => p.Id);
    }
}