using System;
using FlowDesk.TaskBoard.Domain.Enums;
using TaskStatus = FlowDesk.TaskBoard.Domain.Enums.TaskStatus;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record ProjectTaskOverviewDto(
        Guid Id,
        string Title,
        TaskStatus Status,
        TaskPriority Priority,
        Guid? AssigneeId,
        string? AssigneeName,
        DateTimeOffset? DueDateUtc,
        bool IsArchived);
}
