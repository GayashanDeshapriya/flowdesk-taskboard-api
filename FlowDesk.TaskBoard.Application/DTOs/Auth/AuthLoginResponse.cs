namespace FlowDesk.TaskBoard.Application.DTOs.Auth
{
    public sealed record AuthLoginResponse(
    
        string Token,
        Guid UserId,
        string Email,
        string FullName,
        string Role);
    
}
