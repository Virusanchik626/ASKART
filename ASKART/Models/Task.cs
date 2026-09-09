namespace Askart.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }      
        public DateTime Deadline { get; set; }
        public string Priority { get; set; }   
        public int CreatorId { get; set; }
        public int AssigneeUserId { get; set; }
        public string AssigneeName { get; set; }
        public DateTime? CompletedAt { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}