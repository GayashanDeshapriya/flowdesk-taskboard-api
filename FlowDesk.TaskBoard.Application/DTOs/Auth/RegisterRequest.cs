namespace FlowDesk.TaskBoard.Application.DTOs.Auth;

public sealed record RegisterRequest(string Email, string FullName, string Password);
