using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class CreateGroupChatDto
{
    [Required(ErrorMessage = "Введите название группы")]
    [StringLength(100, ErrorMessage = "Название группы не должно превышать 100 символов")]
    public string Name { get; set; } = null!;

    [Required]
    public int CreatorId { get; set; }

    [Required(ErrorMessage = "Укажите тип группы")]
    public string TypeGroup { get; set; } = null!;
}