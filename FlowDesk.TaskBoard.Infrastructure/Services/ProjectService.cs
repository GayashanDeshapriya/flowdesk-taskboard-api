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
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid current user.");

            var userExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == currentUserId, cancellationToken);

            if (!userExists)
                throw new KeyNotFoundException("Current user not found.");

            var project = new Project(request.Name, request.Description);

            _dbContext.Projects.Add(project);
            _dbContext.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = currentUserId,
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
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            if (isAdmin)
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
            else
            {
                return await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .Where(pm => pm.UserId == currentUserId)
                    .Join(_dbContext.Projects,
                        pm => pm.ProjectId,
                        p => p.Id,
                        (pm, p) => new ProjectDetailsResponse(
                            p.Id,
                            p.Name,
                            p.Description,
                            p.CreatedAtUtc,
                            p.UpdatedAtUtc))
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ProjectDetailsResponse> GetByIdAsync(
            Guid projectId,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            var project = await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (!isAdmin)
            {
                var hasAccess = await _dbContext.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUserId, cancellationToken);

                if (!hasAccess)
                    throw new UnauthorizedAccessException("You do not have access to this project.");
            }

            return new ProjectDetailsResponse(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAtUtc,
                project.UpdatedAtUtc);
        }
    }
}
