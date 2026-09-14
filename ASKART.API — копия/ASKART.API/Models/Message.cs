using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class Message
{
    public int IdMessage { get; set; }

    public string Text { get; set; } = null!;

    public int ChatId { get; set; }

    public int SenderId { get; set; }

    public int? ReplyId { get; set; }

    public DateTime SendTime { get; set; }

    public DateTime? EditTime { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
