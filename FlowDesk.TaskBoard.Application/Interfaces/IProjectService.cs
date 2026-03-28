using FlowDesk.TaskBoard.Application.DTOs.Project;

namespace FlowDesk.TaskBoard.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDetailsResponse> CreateAsync(
            CreateProjectRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<ProjectDetailsResponse> GetByIdAsync(
            Guid projectId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);
    }
}
