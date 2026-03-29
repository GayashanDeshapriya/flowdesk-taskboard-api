namespace FlowDesk.TaskBoard.Application.DTOs.Task
{
    public sealed record ProjectTaskListResponse(
        IReadOnlyCollection<ProjectTaskOverviewDto> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages);
}