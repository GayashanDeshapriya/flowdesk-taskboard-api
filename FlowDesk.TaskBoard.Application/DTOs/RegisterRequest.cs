namespace FlowDesk.TaskBoard.Application.DTOs;

public sealed record RegisterRequest(string Email, string FullName, string Password);
