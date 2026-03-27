using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.TaskBoard.Infrastructure.Persistence
{
    public class TaskBoardDbContext : DbContext
    {
        public TaskBoardDbContext() { }
        public TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskBoardDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
