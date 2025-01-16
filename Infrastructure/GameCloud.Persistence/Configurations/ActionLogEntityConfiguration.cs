using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCloud.Persistence.Configurations;

public class ActionLogEntityConfiguration : IEntityTypeConfiguration<ActionLog>
{
    public void Configure(EntityTypeBuilder<ActionLog> builder)
    {
        builder.HasOne(a => a.Function)
            .WithMany(f => f.ActionLogs)
            .HasForeignKey(a => a.FunctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(p => p.Id);
    }
}