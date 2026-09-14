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
    [StringLength(45, MinimumLength = 3,
        ErrorMessage = "Логин должен содержать от 3 до 45 символов")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Введите пароль")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Введите номер телефона")]
    [RegularExpression(
    @"^(?:\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$",
    ErrorMessage = "Введите корректный номер телефона")]
    public string Number { get; set; } = null!;

    [Required(ErrorMessage = "Введите электронную почту")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
    [StringLength(45, ErrorMessage = "Email не должен превышать 45 символов")]
    public string Email { get; set; } = null!;
}