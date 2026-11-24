using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class AddChatMemberDto
{
    [Required]
    public int ChatId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string Role { get; set; } = null!;
}