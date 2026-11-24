using System.ComponentModel.DataAnnotations;

namespace ASKART.API.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Введите фамилию")]
    public string Surname { get; set; } = null!;

    [Required(ErrorMessage = "Введите имя")]
    public string Name { get; set; } = null!;

    public string? Lastname { get; set; }

    [Required(ErrorMessage = "Введите логин")]
    [StringLength(30, MinimumLength = 3,
    ErrorMessage = "Логин (почта) должен содержать от 3 до 30 символов")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(20, MinimumLength = 6,
        ErrorMessage = "Пароль должен содержать от 6 до 20 символов")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Введите номер телефона")]
    [RegularExpression(@"^\+\d{11}$",
        ErrorMessage = "Номер телефона в формате +79991234567")]
    public string Number { get; set; } = null!;

    [Required(ErrorMessage = "Введите электронную почту")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
    [StringLength(45, ErrorMessage = "Email не должен превышать 45 символов")]
    public string Email { get; set; } = null!;
}