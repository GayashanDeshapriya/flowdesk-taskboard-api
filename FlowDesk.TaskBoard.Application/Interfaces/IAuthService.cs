using FlowDesk.TaskBoard.Application.DTOs.Auth;

namespace FlowDesk.TaskBoard.Application.Interfaces;

public interface IAuthService
{
    Task<AuthRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthLoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
