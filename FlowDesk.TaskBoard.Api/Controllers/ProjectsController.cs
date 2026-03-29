using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowDesk.TaskBoard.Application.DTOs.Project;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.TaskBoard.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects")]
    public sealed class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDetailsResponse>> Create(
            [FromBody] CreateProjectRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var created = await _projectService.CreateAsync(request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDetailsResponse>>> GetAll(CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });
            var projects = await _projectService.GetAllAsync(currentUserId, User.IsInRole(nameof(SystemRole.Admin)), cancellationToken);
            return Ok(projects);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var project = await _projectService.GetByIdAsync(
                    id,
                    currentUserId,
                    User.IsInRole(nameof(SystemRole.Admin)),
                    cancellationToken);

                return Ok(project);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var rawUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(rawUserId, out userId);
        }
    }
}
