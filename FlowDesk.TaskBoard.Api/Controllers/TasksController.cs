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
        [Authorize(Policy = "TeamMemberOrAbove")]
        public async Task<ActionResult<TaskDto>> Create(
            [FromBody] CreateTaskRequest request,
            Guid createdById,
            CancellationToken cancellationToken)
        {
            try
            {
                var created = await _taskService.CreateTaskAsync(request, createdById, cancellationToken);
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
        [Authorize(Policy = "TeamMemberOrAbove")]
        public async Task<ActionResult<TaskDto>> GetById(
            Guid taskId,
            CancellationToken cancellationToken)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId, cancellationToken);
            return task is null ? NotFound() : Ok(task);
        }

        [HttpPut("{taskId:guid}")]
        [Authorize(Policy = "TeamMemberOrAbove")]
        public async Task<ActionResult<TaskDto>> Update(
            Guid taskId,
            [FromBody] UpdateTaskRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await _taskService.UpdateTaskAsync(taskId, request, cancellationToken);
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
        }

        [HttpPut("{taskId:guid}/status")]
        [Authorize(Policy = "TeamMemberOrAbove")]
        public async Task<ActionResult<TaskDto>> ChangeStatus(
            Guid taskId,
            [FromBody] ChangeTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await _taskService.ChangeStatusAsync(taskId,request ,cancellationToken);
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
            
        }


        [HttpPatch("{taskId:guid}/archive")]
        [Authorize(Policy = "TeamLeadOrAdmin")]
        public async Task<ActionResult<TaskDto>> Archive(Guid taskId, CancellationToken cancellationToken)
        {
            
            try
            {
                var archived = await _taskService.ArchiveTaskAsync(
                    taskId,              
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

    }
}
