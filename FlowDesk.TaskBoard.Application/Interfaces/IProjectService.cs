using FlowDesk.TaskBoard.Application.DTOs.Project;

namespace FlowDesk.TaskBoard.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDetailsResponse> CreateAsync(
            CreateProjectRequest request,
            Guid createdById,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ProjectDetailsResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<ProjectDetailsResponse> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }
}
