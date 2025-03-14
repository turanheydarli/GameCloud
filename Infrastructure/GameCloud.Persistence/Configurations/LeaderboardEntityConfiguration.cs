using GameCloud.Domain.Entities.Leaderboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GameCloud.Persistence.Configurations
{
    public class LeaderboardEntityConfiguration : IEntityTypeConfiguration<Leaderboard>
    {
        public void Configure(EntityTypeBuilder<Leaderboard> builder)
        {
            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.Name).IsRequired();
            builder.Property(l => l.DisplayName).IsRequired();
            
            // Navigation properties
            builder.HasOne(l => l.Game)
                .WithMany()
                .HasForeignKey(l => l.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(l => l.Records)
                .WithOne(r => r.Leaderboard)
                .HasForeignKey(r => r.LeaderboardId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(l => l.Archives)
                .WithOne()
                .HasForeignKey("LeaderboardId")
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(l => l.ResetStrategy)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<LeaderboardResetStrategy>(v, (JsonSerializerOptions)null));
                    
            builder.HasIndex(l => l.GameId);
            builder.HasIndex(l => l.IsActive);
            builder.HasIndex(l => l.Category);
            builder.HasIndex(l => l.Name);
            builder.HasIndex(l => new { l.GameId, l.Name }).IsUnique();
        }
    }
    
    public class LeaderboardRecordEntityConfiguration : IEntityTypeConfiguration<LeaderboardRecord>
    {
        public void Configure(EntityTypeBuilder<LeaderboardRecord> builder)
        {
            // Primary key
            builder.HasKey(r => r.Id);
            
            // Navigation properties
            builder.HasOne(r => r.Leaderboard)
                .WithMany(l => l.Records)
                .HasForeignKey(r => r.LeaderboardId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(r => r.Player)
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(r => r.Metadata);
            builder.HasIndex(r => r.LeaderboardId);
            builder.HasIndex(r => r.PlayerId);
            builder.HasIndex(r => new { r.LeaderboardId, r.Rank });
            builder.HasIndex(r => new { r.LeaderboardId, r.PlayerId }).IsUnique();
        }
    }
    
    
    public class LeaderboardArchiveEntityConfiguration : IEntityTypeConfiguration<LeaderboardArchive>
    {
        public void Configure(EntityTypeBuilder<LeaderboardArchive> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.HasOne(a => a.Leaderboard)
                .WithMany(l => l.Archives)
                .HasForeignKey("LeaderboardId")
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasIndex("LeaderboardId");
        }
    }
}