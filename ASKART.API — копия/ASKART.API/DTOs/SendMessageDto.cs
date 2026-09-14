using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class SendMessageDto
{
    [Required]
    public int ChatId { get; set; }

    [Required]
    public int SenderId { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Сообщение не должно превышать 200 символов")]
    public string Text { get; set; } = null!;

    public int? ReplyId { get; set; }
}