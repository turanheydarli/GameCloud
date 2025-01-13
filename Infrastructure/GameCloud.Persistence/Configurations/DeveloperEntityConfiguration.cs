using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class DeveloperEntityConfiguration : IEntityTypeConfiguration<Developer>
{
    public void Configure(EntityTypeBuilder<Developer> builder)
    {
        builder.HasMany(d => d.Games)
            .WithOne(g => g.Developer)
            .HasForeignKey(g => g.DeveloperId);

        builder.HasOne(d => d.ProfilePhoto)
            .WithOne()
            .HasForeignKey<Developer>(d => d.ProfilePhotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(p => p.Id);
    }
}