using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.TaskBoard.Infrastructure.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc);

            builder.HasMany(x => x.AssignedTasks)
                .WithOne(x => x.Assignee)
                .HasForeignKey(x => x.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.CreatedTasks)
                .WithOne(x => x.CreatedBy)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
