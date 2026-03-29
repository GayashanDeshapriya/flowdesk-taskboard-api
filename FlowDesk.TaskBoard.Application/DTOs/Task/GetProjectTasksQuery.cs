using FlowDesk.TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = FlowDesk.TaskBoard.Domain.Enums.TaskStatus;

namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed class GetProjectTasksQuery
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? AssigneeId { get; set; }
        public bool IncludeArchived { get; set; } = false;
        public string SortBy { get; set; } = "createdDate";

        [FromQuery(Name = "sortOrder")]
        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
}
