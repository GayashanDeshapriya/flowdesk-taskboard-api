namespace FlowDesk.TaskBoard.Application.DTOs.Auth
{

    public sealed record AuthResponse(
        Guid UserId,
        string Email,
        string FullName,
        string Role);
}
