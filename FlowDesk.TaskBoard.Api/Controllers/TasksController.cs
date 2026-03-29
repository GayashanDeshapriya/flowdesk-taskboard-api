using FlowDesk.TaskBoard.Application.DTOs.Task;
using FlowDesk.TaskBoard.Application.Interfaces;
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
                var created = await _taskService.CreateTaskAsync(request,currentUserId, cancellationToken);
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
        }

        [HttpGet("{taskId:guid}")]
        public async Task<ActionResult<TaskDto>> GetById(Guid taskId, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId, cancellationToken);
            return task is null ? NotFound() : Ok(task);
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
