namespace FlowDesk.TaskBoard.Application.DTOs.Auth
{

    public sealed record AuthRegisterResponse(
        Guid UserId,
        string Email,
        string FullName,
        string Role);
}
