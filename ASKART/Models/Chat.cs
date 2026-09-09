using System.Collections.ObjectModel;

namespace Askart.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } // Дата создания
        public string TypeChat { get; set; }    // "Личный" или "Группа"
        public string TypeGroup { get; set; }   // "Личная" или "Рабочая"
        public int OwnerId { get; set; }
        public bool IsArchived { get; set; }    // Архивация
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public List<ChatMember> Members { get; set; }
        public List<KanbanColumn> KanbanColumns { get; set; }

        public Chat()
        {
            Messages = new ObservableCollection<Message>();
            Tasks = new ObservableCollection<TaskItem>();
            Members = new List<ChatMember>();
            KanbanColumns = new List<KanbanColumn>
            {
                new KanbanColumn { Name = "Новая", Color = "#9E9E9E" },
                new KanbanColumn { Name = "В работе", Color = "#5DD4FF" },
                new KanbanColumn { Name = "Выполнена", Color = "#4CAF50" }
            };
            CreatedAt = DateTime.Now;
            IsArchived = false;
        }

        public string GetUserRole(int userId)
        {
            var member = Members.FirstOrDefault(m => m.UserId == userId);
            return member?.Role ?? "Участник";
        }

        public bool IsAdmin(int userId)
        {
            var role = GetUserRole(userId);
            return role == "Владелец" || role == "Администратор";
        }

        public string ValidateMembersCount()
        {
            if (TypeChat == "Личный" && Members.Count != 2)
                return "Личный чат должен содержать ровно 2 участников.";
            if (TypeChat == "Группа" && Members.Count < 3)
                return "Групповой чат должен содержать не менее 3 участников.";
            return null;
        }
    }

    public class ChatMember
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } // "Владелец", "Администратор", "Участник"
    }

    public class KanbanColumn
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
}