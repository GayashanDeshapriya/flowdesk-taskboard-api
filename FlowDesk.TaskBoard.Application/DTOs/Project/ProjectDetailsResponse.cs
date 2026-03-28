using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.TaskBoard.Application.DTOs.Project
{
    public sealed record ProjectDetailsResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc
    );
}
