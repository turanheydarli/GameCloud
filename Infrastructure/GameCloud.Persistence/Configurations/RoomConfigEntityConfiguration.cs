using GameCloud.Domain.Entities.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GameCloud.Persistence.Configurations;

public class RoomConfigEntityConfiguration : IEntityTypeConfiguration<RoomConfig>
{
    public void Configure(EntityTypeBuilder<RoomConfig> builder)
    {
        builder.HasKey(rc => rc.Id);
        
        builder.Property(rc => rc.CustomConfig)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>());
    }
} 