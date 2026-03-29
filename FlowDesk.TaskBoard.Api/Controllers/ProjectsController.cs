using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowDesk.TaskBoard.Application.DTOs.Project;
using FlowDesk.TaskBoard.Application.DTOs.Task;
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
        private readonly ITaskService _taskService;

        public ProjectsController(IProjectService projectService, ITaskService taskService)
        {
            _projectService = projectService;
            _taskService = taskService;
        }

        [HttpPost]
        [Authorize(Policy = "TeamLeadOrAdmin")]
        public async Task<ActionResult<ProjectDetailsResponse>> Create(
            [FromBody] CreateProjectRequest request,
            Guid createdById,
            CancellationToken cancellationToken)
        {
            
            try
            {
                var created = await _projectService.CreateAsync(request,createdById, cancellationToken);
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
        [Authorize(Policy = "TeamLeadOrAdmin")]
        public async Task<ActionResult<IEnumerable<ProjectDetailsResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
           
            var projects = await _projectService.GetAllAsync(cancellationToken);
            return Ok(projects);
        }


        [HttpGet("{id:guid}")]
        [Authorize(Policy = "TeamLeadOrAdmin")]
        public async Task<ActionResult<ProjectDetailsResponse>> GetById(
            Guid id, 
            CancellationToken cancellationToken)
        {

            try
            {
                var project = await _projectService.GetByIdAsync(
                    id,
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

        [HttpGet("{projectId:guid}/tasks")]
        [Authorize(Policy = "TeamLeadOrAdmin")]
        public async Task<ActionResult<IEnumerable<ProjectTaskOverviewDto>>> GetTasks(
            Guid projectId,
            [FromQuery] bool includeArchived,
            CancellationToken cancellationToken)
        {
            
            try
            {
                var query = new GetProjectTasksQuery { IncludeArchived = includeArchived };
                
                var tasks = await _taskService.GetProjectTasksAsync(
                    projectId,
                    query,                  
                    cancellationToken);

                return Ok(tasks);
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
    }
}
