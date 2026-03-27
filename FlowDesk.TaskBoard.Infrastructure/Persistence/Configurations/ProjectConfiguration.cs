using Microsoft.EntityFrameworkCore;
using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FlowDesk.TaskBoard.Infrastructure.Persistence.Configurations
{
    internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.CreatedAtUtc).IsRequired();
            builder.Property(p => p.UpdatedAtUtc);

            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
