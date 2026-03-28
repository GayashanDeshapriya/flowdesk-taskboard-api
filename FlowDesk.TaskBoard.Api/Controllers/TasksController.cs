using FlowDesk.TaskBoard.Application.DTOs.Task;
using FlowDesk.TaskBoard.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                var created = await _taskService.CreateTaskAsync(request, cancellationToken);
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
    }
}
