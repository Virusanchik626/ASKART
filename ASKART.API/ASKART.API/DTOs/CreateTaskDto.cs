
using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class CreateTaskDto
{
    [Required]
    public int ChatId { get; set; }

    [Required(ErrorMessage = "Введите название задачи")]
    [StringLength(50, ErrorMessage = "Название не должно превышать 50 символов")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Введите описание задачи")]
    public string Describe { get; set; } = null!;

    [Required]
    public int CreatorId { get; set; }

    public int? AssigneUserId { get; set; }

    public DateTime? Deadline { get; set; }

    [Required]
    public string Priority { get; set; } = null!;
}
