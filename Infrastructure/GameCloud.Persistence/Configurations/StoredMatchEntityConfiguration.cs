using GameCloud.Domain.Entities.Matchmaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class StoredMatchEntityConfiguration : IEntityTypeConfiguration<StoredMatch>
{
    public void Configure(EntityTypeBuilder<StoredMatch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Players)
            .WithOne()
            .HasForeignKey(p => p.StoredMatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}