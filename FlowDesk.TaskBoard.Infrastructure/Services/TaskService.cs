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

        public async Task<TaskDto> CreateTaskAsync(
            CreateTaskRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");

            var projectExists = await _dbContext.Projects
                .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (!projectExists)
                throw new KeyNotFoundException($"Project '{request.ProjectId}' was not found.");

            var creatorInProject = await _dbContext.ProjectMembers
                .AsNoTracking()
                .AnyAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == currentUserId, cancellationToken);

            if (!creatorInProject)
                throw new UnauthorizedAccessException("Current user is not a member of the project.");

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
                currentUserId,
                request.Title,
                request.Priority,
                request.DueDateUtc,
                request.AssigneeId,
                request.Description);

            _dbContext.TaskItems.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToDto(task);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(
            Guid taskId, 
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");

            var task = await _dbContext.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task is null)
                return null;

            if(!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);
                if (!hasAccess)
                    throw new UnauthorizedAccessException("Current user is not a member of the project.");
            }

            return ToDto(task);
        }

        public async Task<IReadOnlyCollection<ProjectTaskOverviewDto>> GetProjectTasksAsync(
            Guid projectId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");
            var projectExists = await _dbContext.Projects
                .AnyAsync(p => p.Id == projectId, cancellationToken);
            if (!projectExists)
                throw new KeyNotFoundException($"Project '{projectId}' was not found.");
            if (!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUserId, cancellationToken);
                if (!hasAccess)
                    throw new UnauthorizedAccessException("Current user is not a member of the project.");
            }
            var tasks = await _dbContext.TaskItems
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .Select(t => new ProjectTaskOverviewDto(
                    t.Id,
                    t.Title,
                    t.Status,
                    t.Priority,
                    t.AssigneeId,
                    t.Assignee != null ? t.Assignee.FullName : null,
                    t.DueDateUtc,
                    t.IsArchived))
                .ToListAsync(cancellationToken);
            return tasks;
        }

        public async Task<TaskDto> UpdateTaskAsync(
            Guid taskId,
            UpdateTaskRequest request,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task is null)
                throw new KeyNotFoundException($"Task '{taskId}' was not found.");

            if (!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);

                if (!hasAccess)
                    throw new UnauthorizedAccessException("Current user is not a member of the project.");
            }

            if (task.IsArchived)
                throw new InvalidOperationException("Archived tasks cannot be updated.");

            if (request.AssigneeId == Guid.Empty)
                throw new ArgumentException("AssigneeId cannot be an empty GUID.", nameof(request.AssigneeId));

            if (request.AssigneeId.HasValue)
            {
                var assigneeExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == request.AssigneeId.Value, cancellationToken);

                if (!assigneeExists)
                    throw new KeyNotFoundException($"Assignee '{request.AssigneeId.Value}' was not found.");

                var assigneeInProject = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == request.AssigneeId.Value, cancellationToken);

                if (!assigneeInProject)
                    throw new InvalidOperationException("Assignee must be a member of the project.");
            }

            task.UpdateDetails(request.Title, request.Description, request.Priority, request.DueDateUtc);
            task.AssignTo(request.AssigneeId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToDto(task);
        }

        public async Task<TaskDto> ChangeStatusAsync(
           Guid taskId,
           ChangeTaskStatusRequest request,
           Guid currentUserId,
           bool isAdmin,
           CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");

            if (!Enum.IsDefined(request.NewStatus))
                throw new ArgumentException("Invalid task status value.", nameof(request.NewStatus));

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task is null)
                throw new KeyNotFoundException($"Task '{taskId}' was not found.");

            if (!Enum.IsDefined(task.Status))
                throw new InvalidOperationException("Task has an invalid current status.");

            if (!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);

                if (!hasAccess)
                    throw new UnauthorizedAccessException("Current user is not a member of the project.");
            }

            task.ChangeStatus(request.NewStatus);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToDto(task);
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
