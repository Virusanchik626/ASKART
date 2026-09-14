using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class CreatePrivateChatDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int OtherUserId { get; set; }
}