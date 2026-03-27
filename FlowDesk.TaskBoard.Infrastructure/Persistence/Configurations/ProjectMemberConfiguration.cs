using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.TaskBoard.Infrastructure.Persistence.Configurations
{
    internal sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProjectId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.RoleInProject).IsRequired().HasMaxLength(50);

            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc);

            builder.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
            builder.HasIndex(x => x.UserId);

            builder.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}