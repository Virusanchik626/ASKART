
using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class UpdateTaskDto
{
    [Required(ErrorMessage = "Введите название задачи")]
    [StringLength(50, ErrorMessage = "Название не должно превышать 50 символов")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Введите описание задачи")]
    public string Describe { get; set; } = null!;

    [Required]
    public string Status { get; set; } = null!;

    [Required]
    public string Priority { get; set; } = null!;

    public DateTime? Deadline { get; set; }
}
