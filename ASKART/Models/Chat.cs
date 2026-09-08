using Askart.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Askart.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeChat { get; set; } // "Личный" или "Группа"
        public string TypeGroup { get; set; } // "Личная" или "Рабочая"
        public int OwnerId { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public List<ChatMember> Members { get; set; }

        public Chat()
        {
            Messages = new ObservableCollection<Message>();
            Tasks = new ObservableCollection<TaskItem>();
            Members = new List<ChatMember>();
        }
    }

    public class ChatMember
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } // "Управляющий" или "Работник"
    }
}