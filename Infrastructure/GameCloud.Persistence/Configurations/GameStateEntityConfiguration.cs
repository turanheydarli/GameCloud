using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class GameStateEntityConfiguration : IEntityTypeConfiguration<GameState>
{
    public void Configure(EntityTypeBuilder<GameState> builder)
    {
        builder.HasOne<Session>()
            .WithMany()
            .HasForeignKey(gs => gs.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasKey(p => p.Id);
    }
}