using FlowDesk.TaskBoard.Domain.Enums;
using TaskStatus = FlowDesk.TaskBoard.Domain.Enums.TaskStatus;

namespace FlowDesk.TaskBoard.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        private TaskItem() { }

        public TaskItem(
            Guid projectId,
            string title,
            TaskPriority priority,
            DateTimeOffset? dueDateUtc = null,
            Guid? assigneeId = null,
            string? description = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("ProjectId is required.", nameof(projectId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (dueDateUtc.HasValue && dueDateUtc.Value < DateTimeOffset.UtcNow)
                throw new ArgumentException("Due date cannot be in the past.", nameof(dueDateUtc));

            ProjectId = projectId;
            Title = title.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Priority = priority;
            DueDateUtc = dueDateUtc;
            AssigneeId = assigneeId;
            Status = TaskStatus.ToDo;
            IsArchived = false;
        }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTimeOffset? DueDateUtc { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public bool IsArchived { get; set; }
        public DateTimeOffset? ArchivedAtUtc { get; set; }

        public void UpdateDetails(string title, string? description, TaskPriority priority, DateTimeOffset? dueDateUtc)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (dueDateUtc.HasValue && dueDateUtc.Value < DateTimeOffset.UtcNow)
                throw new ArgumentException("Due date cannot be in the past.", nameof(dueDateUtc));

            Title = title.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Priority = priority;
            DueDateUtc = dueDateUtc;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        public void AssignTo(Guid? assigneeId)
        {
            AssigneeId = assigneeId;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        public void ChangeStatus(TaskStatus newStatus)
        {
            if (IsArchived)
                throw new InvalidOperationException("Archived tasks cannot change status.");

            var valid =
                (Status == TaskStatus.ToDo && (newStatus == TaskStatus.InProgress || newStatus == TaskStatus.Cancelled)) ||
                (Status == TaskStatus.InProgress && (newStatus == TaskStatus.Done || newStatus == TaskStatus.Cancelled)) ||
                (Status == TaskStatus.Done && newStatus == TaskStatus.Done) ||
                (Status == TaskStatus.Cancelled && newStatus == TaskStatus.Cancelled);

            if (!valid)
                throw new InvalidOperationException($"Invalid status transition: {Status} -> {newStatus}");

            Status = newStatus;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        public void Archive()
        {
            if (Status is not (TaskStatus.Done or TaskStatus.Cancelled))
                throw new InvalidOperationException("Only Done or Cancelled tasks can be archived.");

            IsArchived = true;
            ArchivedAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}

