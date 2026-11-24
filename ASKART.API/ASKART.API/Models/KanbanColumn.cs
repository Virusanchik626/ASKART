using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class KanbanColumn
{
    public int IdColumn { get; set; }

    public int ChatId { get; set; }

    public string Name { get; set; } = null!;

    public string Color { get; set; } = null!;

    public int Position { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
