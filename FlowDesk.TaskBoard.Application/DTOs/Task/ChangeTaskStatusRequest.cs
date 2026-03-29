using FlowDesk.TaskBoard.Domain.Enums;
using TaskStatus = FlowDesk.TaskBoard.Domain.Enums.TaskStatus;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record ChangeTaskStatusRequest(TaskStatus NewStatus);
}