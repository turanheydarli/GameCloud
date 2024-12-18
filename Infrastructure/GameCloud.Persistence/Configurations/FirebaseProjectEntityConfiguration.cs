using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class FirebaseProjectEntityConfiguration : IEntityTypeConfiguration<FirebaseProject>
{
    public void Configure(EntityTypeBuilder<FirebaseProject> builder)
    {
        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(fp => fp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(p => p.Id);
    }
}