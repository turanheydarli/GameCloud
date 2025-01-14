using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class FunctionConfigEntityConfiguration : IEntityTypeConfiguration<FunctionConfig>
{
    public void Configure(EntityTypeBuilder<FunctionConfig> builder)
    {
        builder.HasOne<Game>()
            .WithMany(g => g.Functions)
            .HasForeignKey(fc => fc.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(p => p.Id);
    }
}