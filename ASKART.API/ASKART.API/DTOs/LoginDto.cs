using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Введите логин, почту или номер телефона")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Введите пароль")]
    public string Password { get; set; } = null!;
}