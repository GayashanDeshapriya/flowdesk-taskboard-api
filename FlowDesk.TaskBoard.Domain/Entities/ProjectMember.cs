using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.TaskBoard.Domain.Entities
{
    public class ProjectMember : BaseEntity
    {
        public required Guid ProjectId { get; set; }
        public required Guid UserId { get; set; }
        public required string RoleInProject { get; set; }
    }
}
