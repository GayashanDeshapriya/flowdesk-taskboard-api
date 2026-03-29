using FlowDesk.TaskBoard.Application.DTOs.Task;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FlowDesk.TaskBoard.Api.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public sealed class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var created = await _taskService.CreateTaskAsync(request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { taskId = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpGet("{taskId:guid}")]
        public async Task<ActionResult<TaskDto>> GetById(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var task = await _taskService.GetTaskByIdAsync(
                    taskId,
                    currentUserId,
                    User.IsInRole(nameof(SystemRole.Admin)),
                    cancellationToken);

                return task is null ? NotFound() : Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }


        [HttpPut("{taskId:guid}")]
        public async Task<ActionResult<TaskDto>> Update(Guid taskId, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var updated = await _taskService.UpdateTaskAsync(
                    taskId,
                    request,
                    currentUserId,
                    User.IsInRole(nameof(SystemRole.Admin)),
                    cancellationToken);

                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }


        [HttpPatch("{taskId:guid}/status")]
        public async Task<ActionResult<TaskDto>> ChangeStatus(
            Guid taskId,
            [FromBody] ChangeTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var updated = await _taskService.ChangeStatusAsync(
                    taskId,
                    request,
                    currentUserId,
                    User.IsInRole(nameof(SystemRole.Admin)),
                    cancellationToken);

                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }


        [HttpPatch("{taskId:guid}/archive")]
        public async Task<ActionResult<TaskDto>> Archive(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token subject." });

            try
            {
                var archived = await _taskService.ArchiveTaskAsync(
                    taskId,
                    currentUserId,
                    User.IsInRole(nameof(SystemRole.Admin)),
                    cancellationToken);

                return Ok(archived);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
