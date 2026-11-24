using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class Chat
{
    public int IdChat { get; set; }

    public string Name { get; set; } = null!;

    public string TypeChat { get; set; } = null!;

    public string? TypeGroup { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Chatmember> Chatmembers { get; set; } = new List<Chatmember>();

    public virtual ICollection<KanbanColumn> KanbanColumns { get; set; } = new List<KanbanColumn>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
