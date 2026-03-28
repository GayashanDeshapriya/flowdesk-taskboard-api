using FlowDesk.TaskBoard.Application.DTOs.Task;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Entities;
using FlowDesk.TaskBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.TaskBoard.Infrastructure.Services
{
    public sealed class TaskService : ITaskService
    {
        private readonly TaskBoardDbContext _dbContext;

        public TaskService(TaskBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
        {
            var projectExists = await _dbContext.Projects
                .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (!projectExists)
                throw new KeyNotFoundException($"Project '{request.ProjectId}' was not found.");

            var creatorExists = await _dbContext.Users
                .AnyAsync(u => u.Id == request.CreatedById, cancellationToken);

            if (!creatorExists)
                throw new KeyNotFoundException($"Creator '{request.CreatedById}' was not found.");

            if (request.AssigneeId.HasValue)
            {
                var assigneeExists = await _dbContext.Users
                    .AnyAsync(u => u.Id == request.AssigneeId.Value, cancellationToken);

                if (!assigneeExists)
                    throw new KeyNotFoundException($"Assignee '{request.AssigneeId.Value}' was not found.");

                var assigneeInProject = await _dbContext.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == request.AssigneeId.Value, cancellationToken);

                if (!assigneeInProject)
                    throw new InvalidOperationException("Assignee must be a member of the project.");
            }

            var task = new TaskItem(
                request.ProjectId,
                request.CreatedById,
                request.Title,
                request.Priority,
                request.DueDateUtc,
                request.AssigneeId,
                request.Description);

            _dbContext.TaskItems.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToDto(task);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var task = await _dbContext.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            return task is null ? null : ToDto(task);
        }

        private static TaskDto ToDto(TaskItem task) =>
            new(
                task.Id,
                task.ProjectId,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.DueDateUtc,
                task.CreatedById,
                task.AssigneeId,
                task.IsArchived,
                task.CreatedAtUtc,
                task.UpdatedAtUtc);
    }
}
