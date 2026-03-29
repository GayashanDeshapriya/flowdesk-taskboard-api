using FlowDesk.TaskBoard.Application.DTOs.Project;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Entities;
using FlowDesk.TaskBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.TaskBoard.Infrastructure.Services
{
    public sealed class ProjectService : IProjectService
    {
        private readonly TaskBoardDbContext _dbContext;

        public ProjectService(TaskBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProjectDetailsResponse> CreateAsync(
            CreateProjectRequest request,
            Guid createdById,
            CancellationToken cancellationToken = default)
        {

            var userExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == createdById, cancellationToken);

            if (!userExists)
                throw new KeyNotFoundException("Current user not found.");

            var project = new Project(request.Name, request.Description);

            _dbContext.Projects.Add(project);
            _dbContext.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = createdById,
                RoleInProject = "Owner"
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ProjectDetailsResponse(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAtUtc,
                project.UpdatedAtUtc);
        }

        public async Task<IEnumerable<ProjectDetailsResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {

            return await _dbContext.Projects
                .AsNoTracking()
                .Select(p => new ProjectDetailsResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CreatedAtUtc,
                    p.UpdatedAtUtc))
                .ToListAsync(cancellationToken);
        }

    


        public async Task<ProjectDetailsResponse> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            return new ProjectDetailsResponse(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAtUtc,
                project.UpdatedAtUtc);
        }
    }
}
