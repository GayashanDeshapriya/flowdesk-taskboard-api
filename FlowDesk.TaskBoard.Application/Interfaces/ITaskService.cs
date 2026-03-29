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

        Task<IReadOnlyCollection<ProjectTaskOverviewDto>> GetProjectTasksAsync(
            Guid projectId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);
    }
}
