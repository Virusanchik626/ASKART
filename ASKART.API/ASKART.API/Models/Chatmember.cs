using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class Chatmember
{
    public int ChatId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public virtual Chat Chat { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
