using FlowDesk.TaskBoard.Application.DTOs.Task;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Entities;
using FlowDesk.TaskBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.TaskBoard.Infrastructure.Services
{
    public sealed class TaskService : ITaskService
    {
        private const int MaxPageSize = 100;
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

            if (!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);

                if (!hasAccess)
                    throw new UnauthorizedAccessException("Current user is not a member of the project.");
            }

            return ToDto(task);
        }

        public async Task<ProjectTaskListResponse> GetProjectTasksAsync(
            Guid projectId,
            GetProjectTasksQuery query,
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

            if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
                throw new ArgumentException("Invalid status filter.", nameof(query.Status));

            if (query.Priority.HasValue && !Enum.IsDefined(query.Priority.Value))
                throw new ArgumentException("Invalid priority filter.", nameof(query.Priority));

            if (query.AssigneeId == Guid.Empty)
                throw new ArgumentException("AssigneeId cannot be an empty GUID.", nameof(query.AssigneeId));

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
            if (pageSize > MaxPageSize)
                pageSize = MaxPageSize;

            var sortBy = (query.SortBy ?? "createdDate").Trim().ToLowerInvariant();
            var sortDirection = (query.SortDirection ?? "desc").Trim().ToLowerInvariant();

            if (sortBy is not ("duedate" or "createddate"))
                throw new ArgumentException("SortBy must be either 'dueDate' or 'createdDate'.", nameof(query.SortBy));

            if (sortDirection is not ("asc" or "desc"))
                throw new ArgumentException("SortDirection must be either 'asc' or 'desc'.", nameof(query.SortDirection));

            var taskQuery = _dbContext.TaskItems
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId);

            if (!query.IncludeArchived)
                taskQuery = taskQuery.Where(t => !t.IsArchived);

            if (query.Status.HasValue)
                taskQuery = taskQuery.Where(t => t.Status == query.Status.Value);

            if (query.Priority.HasValue)
                taskQuery = taskQuery.Where(t => t.Priority == query.Priority.Value);

            if (query.AssigneeId.HasValue)
                taskQuery = taskQuery.Where(t => t.AssigneeId == query.AssigneeId.Value);

            taskQuery = (sortBy, sortDirection) switch
            {
                ("duedate", "asc") => taskQuery
                    .OrderBy(t => t.DueDateUtc == null)
                    .ThenBy(t => t.DueDateUtc)
                    .ThenByDescending(t => t.CreatedAtUtc),

                ("duedate", "desc") => taskQuery
                    .OrderBy(t => t.DueDateUtc == null)
                    .ThenByDescending(t => t.DueDateUtc)
                    .ThenByDescending(t => t.CreatedAtUtc),

                ("createddate", "asc") => taskQuery
                    .OrderBy(t => t.CreatedAtUtc),

                _ => taskQuery
                    .OrderByDescending(t => t.CreatedAtUtc)
            };

            var totalCount = await taskQuery.CountAsync(cancellationToken);
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await taskQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return new ProjectTaskListResponse(
                items,
                pageNumber,
                pageSize,
                totalCount,
                totalPages);
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

        public async Task<TaskDto> ArchiveTaskAsync(
            Guid taskId,
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

            task.Archive();

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
