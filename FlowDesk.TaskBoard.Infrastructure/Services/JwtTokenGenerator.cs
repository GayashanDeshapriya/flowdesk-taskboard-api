using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FlowDesk.TaskBoard.Application.Common;
using FlowDesk.TaskBoard.Application.Interfaces;
using FlowDesk.TaskBoard.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.TaskBoard.Infrastructure.Services
{
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;

            if (string.IsNullOrWhiteSpace(_jwtOptions.Key) || _jwtOptions.Key.Length < 32)
                throw new InvalidOperationException("JWT key must be at least 32 characters.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        public (string AccessToken, DateTimeOffset ExpiresAtUtc) GenerateToken(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: _signingCredentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return (accessToken, expiresAt);
        }
    }
}
