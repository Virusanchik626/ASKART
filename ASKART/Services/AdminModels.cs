namespace Askart.Services
{
    // ───────────────────────── DTO ответов ─────────────────────────

    public class PagedResult<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Lastname { get; set; }
        public string Login { get; set; } = "";
        public string? Email { get; set; }
        public string Number { get; set; } = "";
        public DateTime RegistrationDate { get; set; }
        public bool IsAdmin { get; set; }
        public int ChatsCount { get; set; }
        public int MessagesCount { get; set; }
        public int TasksCreatedCount { get; set; }

        public string FullName
        {
            get { return (Surname + " " + Name + " " + Lastname).Trim(); }
        }
    }

    public class AdminChatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string TypeChat { get; set; } = "";
        public string? TypeGroup { get; set; }
        public string OwnerName { get; set; } = "";
        public int MembersCount { get; set; }
        public int MessagesCount { get; set; }
        public int TasksCount { get; set; }
        public DateTime? LastMessageTime { get; set; }

        public string Summary => "Участников: " + MembersCount + " • Сообщений: " + MessagesCount +
            (TypeGroup == "Рабочая" ? " • Задач: " + TasksCount : "");

        public string TypeText
        {
            get
            {
                if (TypeGroup != null)
                {
                    return TypeChat + " • " + TypeGroup;
                }
                return TypeChat;
            }
        }
    }

    public class AdminMessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public string Text { get; set; } = "";
        public int? ReplyId { get; set; }
        public DateTime SendTime { get; set; }
        public DateTime? EditTime { get; set; }
    }

    public class AdminStatsDto
    {
        public UsersStats Users { get; set; } = new UsersStats();
        public ChatsStats Chats { get; set; } = new ChatsStats();
        public MessagesStats Messages { get; set; } = new MessagesStats();
        public TasksStats Tasks { get; set; } = new TasksStats();
        public List<DayCount> MessagesPerDay { get; set; } = new List<DayCount>();
        public List<NameCount> TopUsers { get; set; } = new List<NameCount>();
        public List<NameCount> TopChats { get; set; } = new List<NameCount>();
    }

    public class UsersStats
    {
        public int Total { get; set; }
        public int Admins { get; set; }
        public int New7Days { get; set; }
        public int New30Days { get; set; }
        public int Active7Days { get; set; }
    }

    public class ChatsStats
    {
        public int Total { get; set; }
        public int PrivateChats { get; set; }
        public int PersonalGroups { get; set; }
        public int WorkGroups { get; set; }
    }

    public class MessagesStats
    {
        public int Total { get; set; }
        public int Today { get; set; }
        public int Last7Days { get; set; }
        public int Edited { get; set; }
    }

    public class TasksStats
    {
        public int Total { get; set; }
        public int Created { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
    }

    public class DayCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class NameCount
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

}
