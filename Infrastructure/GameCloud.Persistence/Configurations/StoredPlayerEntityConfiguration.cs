using GameCloud.Domain.Entities;
using GameCloud.Domain.Entities.Matchmaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class StoredPlayerEntityConfiguration : IEntityTypeConfiguration<StoredPlayer>
{
    public void Configure(EntityTypeBuilder<StoredPlayer> builder)
    {
        builder.HasKey(x => ((BaseEntity)x).Id);

        builder.HasOne<Player>()
            .WithOne()
            .HasForeignKey<StoredPlayer>(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}