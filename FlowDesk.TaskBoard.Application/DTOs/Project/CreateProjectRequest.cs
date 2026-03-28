namespace FlowDesk.TaskBoard.Application.DTOs.Project
{
    public sealed record CreateProjectRequest(
        string Name,
        string Description
        );
}
