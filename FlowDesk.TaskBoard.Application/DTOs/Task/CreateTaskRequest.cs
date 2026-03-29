using FlowDesk.TaskBoard.Domain.Enums;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record CreateTaskRequest(
        Guid ProjectId,
        string Title,
        TaskPriority Priority,
        DateTimeOffset? DueDateUtc,
        Guid? AssigneeId,
        string? Description);
}
