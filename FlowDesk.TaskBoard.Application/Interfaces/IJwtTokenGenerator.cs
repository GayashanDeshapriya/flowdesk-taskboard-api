using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowDesk.TaskBoard.Domain.Entities;

namespace FlowDesk.TaskBoard.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string AccessToken, DateTimeOffset ExpiresAtUtc) GenerateToken(User user);
    }
}
