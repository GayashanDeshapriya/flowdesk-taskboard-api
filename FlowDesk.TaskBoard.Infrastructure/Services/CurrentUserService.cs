using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowDesk.TaskBoard.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FlowDesk.TaskBoard.Infrastructure.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
    
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                var principal = _httpContextAccessor.HttpContext?.User;
                if (principal is null)
                    return null;

                var candidate = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

                return Guid.TryParse(candidate, out var userId) ? userId : null;
            }
        }
    }
}
