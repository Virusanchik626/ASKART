namespace Askart.Services
{
    // Что приходит с сервера при логине
    public class ApiLoginUser
    {
        public bool IsAdmin { get; set; }
        public int Id { get; set; }
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string Number { get; set; } = "";
    }

    // Что приходит из GET api/Users (там поле называется idUser)
    public class ApiUser
    {
        public int IdUser { get; set; }
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Number { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime DataReg { get; set; }
    }

    public class ApiChat
    {
        public int ChatId { get; set; }
        public string Name { get; set; } = "";
        public string TypeChat { get; set; } = "";
        public string TypeGroup { get; set; } = "";
    }

    public class ApiMember
    {
        public int UserId { get; set; }
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Login { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class ApiMessage
    {
        public int MessageId { get; set; }
        public string Text { get; set; } = "";
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public int? ReplyId { get; set; }
        public DateTime SendTime { get; set; }
        public DateTime? EditTime { get; set; }
    }

    public class ApiTask
    {
        public int TaskId { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; } = "";
        public string Describe { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public DateTime Deadline { get; set; }
        public int CreatorId { get; set; }
        public int AssigneUserId { get; set; }
    }

    public class ApiColumn
    {
        public int ColumnId { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; } = "";
        public string Color { get; set; } = "";
        public int Position { get; set; }
    }
}