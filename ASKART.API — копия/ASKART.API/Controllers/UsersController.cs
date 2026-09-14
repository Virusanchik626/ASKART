using ASKART.API.Data;
using ASKART.API.DTOs;
using ASKART.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AskartContext _context;

    public UsersController(AskartContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Login == dto.Login ||
                u.Email == dto.Login ||
                u.Number == dto.Login);

        if (user == null || user.Password != dto.Password)
        {
            return Unauthorized(new
            {
                message = "Неверный логин, почта, номер телефона или пароль."
            });
        }

        return Ok(new
        {
            id = user.IdUser,
            surname = user.Surname,
            name = user.Name,
            lastname = user.Lastname,
            login = user.Login,
            email = user.Email,
            number = user.Number
        });
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var loginExists = await _context.Users
            .AnyAsync(u => u.Login == dto.Login);

        if (loginExists)
        {
            return BadRequest(new
            {
                message = "Пользователь с таким логином уже существует."
            });
        }

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
        {
            return BadRequest(new
            {
                message = "Пользователь с такой электронной почтой уже существует."
            });
        }

        var numberExists = await _context.Users
            .AnyAsync(u => u.Number == dto.Number);

        if (numberExists)
        {
            return BadRequest(new
            {
                message = "Пользователь с таким номером телефона уже существует."
            });
        }

        var lastId = await _context.Users
            .Select(u => (int?)u.IdUser)
            .MaxAsync() ?? 0;

        var user = new User
        {
            IdUser = lastId + 1,
            Surname = dto.Surname,
            Name = dto.Name,
            Lastname = dto.Lastname,
            Login = dto.Login,
            Password = dto.Password,
            Number = dto.Number,
            Email = dto.Email,
            DataReg = DateTime.Now
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(user);
    }
}