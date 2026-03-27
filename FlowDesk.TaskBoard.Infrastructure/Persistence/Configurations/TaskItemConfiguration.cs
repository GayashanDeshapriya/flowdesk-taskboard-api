using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.TaskBoard.Infrastructure.Persistence.Configurations
{
    internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("TaskItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Status).IsRequired().HasConversion<int>();
            builder.Property(x => x.Priority).IsRequired().HasConversion<int>();

            builder.Property(x => x.ProjectId).IsRequired();
            builder.Property(x => x.CreatedById).IsRequired();

            builder.Property(x => x.IsArchived).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc);

            builder.HasIndex(x => x.ProjectId);
            builder.HasIndex(x => x.AssigneeId);
            builder.HasIndex(x => x.CreatedById);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsArchived);
            builder.HasIndex(x => new { x.ProjectId, x.Status, x.IsArchived });
        }
    }
}
