using FlowDesk.TaskBoard.Domain.Enums;
using TaskStatus = FlowDesk.TaskBoard.Domain.Enums.TaskStatus;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record TaskDto(
    Guid Id,
        Guid ProjectId,
        string Title,
        string? Description,
        TaskStatus Status,
        TaskPriority Priority,
        DateTimeOffset? DueDateUtc,
        Guid CreatedById,
        Guid? AssigneeId,
        bool IsArchived,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc);
}
