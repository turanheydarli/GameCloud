using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class ImageDocumentEntityConfiguration : IEntityTypeConfiguration<ImageDocument>
{
    public void Configure(EntityTypeBuilder<ImageDocument> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasMany(d => d.Variants)
            .WithOne()
            .HasForeignKey(v => v.ImageDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Url)
            .IsRequired()
            .HasMaxLength(2048);
        builder.Property(p => p.FileType)
            .HasMaxLength(50);
        builder.Property(p => p.Width)
            .IsRequired();
        builder.Property(p => p.Height)
            .IsRequired();
        builder.Property(p => p.StorageProvider)
            .HasMaxLength(100);
    }
}