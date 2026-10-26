using System.Collections.ObjectModel;

namespace Askart.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string TypeChat { get; set; }    
        public string TypeGroup { get; set; }   
        public int OwnerId { get; set; }
        public bool IsArchived { get; set; }    
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public List<ChatMember> Members { get; set; }
        public List<KanbanColumn> KanbanColumns { get; set; }

        public Chat()
        {
            Messages = new ObservableCollection<Message>();
            Tasks = new ObservableCollection<TaskItem>();
            Members = new List<ChatMember>();
            KanbanColumns = new List<KanbanColumn>();

            KanbanColumn column1 = new KanbanColumn();
            column1.Name = "Новая";
            column1.Color = "#9E9E9E";
            KanbanColumns.Add(column1);

            KanbanColumn column2 = new KanbanColumn();
            column2.Name = "В работе";
            column2.Color = "#5DD4FF";
            KanbanColumns.Add(column2);

            KanbanColumn column3 = new KanbanColumn();
            column3.Name = "Выполнена";
            column3.Color = "#4CAF50";
            KanbanColumns.Add(column3);

            CreatedAt = DateTime.Now;
            IsArchived = false;
        }

        
        public string GetUserRole(int userId)
        {
            foreach (ChatMember member in Members)
            {
                if (member.UserId == userId)
                {
                    return member.Role;
                }
            }
            return "Участник";
        }

        public bool IsAdmin(int userId)
        {
            string role = GetUserRole(userId);
            if (role == "Владелец" || role == "Администратор")
            {
                return true;
            }
            return false;
        }

        public string ValidateMembersCount()
        {
            if (TypeChat == "Личный" && Members.Count != 2)
            {
                return "Личный чат должен содержать ровно 2 участников.";
            }
            if (TypeChat == "Группа" && Members.Count < 3)
            {
                return "Групповой чат должен содержать не менее 3 участников.";
            }
            return null;
        }
    }

    public class ChatMember
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } 
    }

    public class KanbanColumn
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
