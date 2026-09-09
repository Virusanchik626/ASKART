namespace Askart.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public int? ReplyId { get; set; }           
        public DateTime SendTime { get; set; }
        public DateTime? EditTime { get; set; }     
        public bool IsPinned { get; set; }        
        public string SenderName { get; set; }
        public Message ReplyTo { get; set; }
    }
}