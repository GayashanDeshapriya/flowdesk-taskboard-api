using FlowDesk.TaskBoard.Domain.Enums;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record UpdateTaskRequest(
        string Title,
        string? Description,
        TaskPriority Priority,
        DateTimeOffset? DueDateUtc,
        Guid? AssigneeId);
}