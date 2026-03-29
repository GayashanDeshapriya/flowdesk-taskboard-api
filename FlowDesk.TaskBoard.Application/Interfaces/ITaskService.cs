using FlowDesk.TaskBoard.Application.DTOs.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.TaskBoard.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(
            CreateTaskRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<TaskDto?> GetTaskByIdAsync(
            Guid taskId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

        Task<ProjectTaskListResponse> GetProjectTasksAsync(
            Guid projectId,
            GetProjectTasksQuery query,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

        Task<TaskDto> UpdateTaskAsync(
            Guid taskId,
            UpdateTaskRequest request,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

        Task<TaskDto> ChangeStatusAsync(
            Guid taskId,
            ChangeTaskStatusRequest request,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

        Task<TaskDto> ArchiveTaskAsync(
            Guid taskId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);
    }
}
