using System;
using System.Collections.Generic;

namespace ASKART.API.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Lastname { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public byte[]? Avatar { get; set; }

    public string Number { get; set; } = null!;

    public DateTime DataReg { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Chatmember> Chatmembers { get; set; } = new List<Chatmember>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
