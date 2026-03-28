using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.TaskBoard.Application.Common
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public required string Key { get; init; }
        public int AccessTokenMinutes { get; init; } = 60;
    }
}
