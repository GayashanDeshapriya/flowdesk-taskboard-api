using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.TaskBoard.Domain.Enums
{
    public enum TaskStatus
    {
        ToDo = 1,
        InProgress = 2,
        Done = 3,
        Cancelled = 4,
        Archived = 5
    }
}
