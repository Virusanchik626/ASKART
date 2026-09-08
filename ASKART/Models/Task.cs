namespace Askart.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // "Новая", "В работе", "Выполнена"
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } // "Низкий", "Средний", "Высокий"
        public int CreatorId { get; set; }
        public int AssigneeUserId { get; set; }
        public string AssigneeName { get; set; }
    }
}