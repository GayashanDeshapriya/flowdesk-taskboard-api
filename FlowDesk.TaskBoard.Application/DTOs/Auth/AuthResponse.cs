namespace FlowDesk.TaskBoard.Application.DTOs.Auth
{

    public sealed record AuthResponse(
        string AccessToken,
        DateTimeOffset ExpiresAtUtc,
        Guid UserId,
        string Email,
        string FullName,
        string Role);
}
