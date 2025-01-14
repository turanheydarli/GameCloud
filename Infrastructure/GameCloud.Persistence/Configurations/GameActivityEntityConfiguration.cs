using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class GameActivityEntityConfiguration : IEntityTypeConfiguration<GameActivity>
{
    public void Configure(EntityTypeBuilder<GameActivity> builder)
    {
        builder.HasOne(ga => ga.Game)
            .WithMany(g => g.Activities)
            .HasForeignKey(ga => ga.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(p => p.Id);
    }
}