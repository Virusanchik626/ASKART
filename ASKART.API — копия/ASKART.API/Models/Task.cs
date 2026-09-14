using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class Task
{
    public int IdTask { get; set; }

    public int ChatId { get; set; }

    public string Name { get; set; } = null!;

    public string? Describe { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Deadline { get; set; }

    public string Priority { get; set; } = null!;

    public int CreatorId { get; set; }

    public int AssigneUserId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User Creator { get; set; } = null!;
}
