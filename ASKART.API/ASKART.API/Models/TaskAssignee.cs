using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class TaskAssignee
{
    public int TaskId { get; set; }

    public int UserId { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
