using GameCloud.Domain.Entities.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GameCloud.Persistence.Configurations;

public class RoomEntityConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.HasOne(r => r.Config)
            .WithOne()
            .HasForeignKey<RoomConfig>("RoomId")
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure JSON serialization for Dictionary and List properties
        builder.Property(r => r.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>());

        builder.Property(r => r.PlayerIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

        builder.Property(r => r.SpectatorIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());
                
        // Create indexes for common query patterns
        builder.HasIndex(r => r.GameId);
        builder.HasIndex(r => r.CreatorId);
        builder.HasIndex(r => r.State);
    }
} 