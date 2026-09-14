using ASKART.API.Data;
using ASKART.API.DTOs;
using ASKART.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatMembersController : ControllerBase
{
    private readonly AskartContext _context;

    public ChatMembersController(AskartContext context)
    {
        _context = context;
    }

    // Добавить пользователя в чат
    [HttpPost]
    public async Task<IActionResult> AddMember(AddChatMemberDto dto)
    {
        // Проверяем существование чата
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == dto.ChatId);

        if (!chatExists)
        {
            return NotFound(new
            {
                message = "Чат не найден."
            });
        }

        // Проверяем существование пользователя
        var userExists = await _context.Users
            .AnyAsync(u => u.IdUser == dto.UserId);

        if (!userExists)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        // Проверяем, не состоит ли уже пользователь в чате
        var memberExists = await _context.Chatmembers
            .AnyAsync(cm =>
                cm.ChatId == dto.ChatId &&
                cm.UserId == dto.UserId);

        if (memberExists)
        {
            return BadRequest(new
            {
                message = "Пользователь уже состоит в этом чате."
            });
        }

        var chatMember = new Chatmember
        {
            ChatId = dto.ChatId,
            UserId = dto.UserId,
            Role = dto.Role
        };

        _context.Chatmembers.Add(chatMember);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Пользователь добавлен в чат.",
            chatId = chatMember.ChatId,
            userId = chatMember.UserId,
            role = chatMember.Role
        });
    }


    // Получить участников чата
    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetMembers(int chatId)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return NotFound(new
            {
                message = "Чат не найден."
            });
        }

        var members = await _context.Chatmembers
            .Where(cm => cm.ChatId == chatId)
            .Include(cm => cm.User)
            .Select(cm => new
            {
                userId = cm.UserId,
                surname = cm.User.Surname,
                name = cm.User.Name,
                lastname = cm.User.Lastname,
                login = cm.User.Login,
                role = cm.Role
            })
            .ToListAsync();

        return Ok(members);
    }


    // Удалить пользователя из чата
    [HttpDelete("{chatId}/{userId}")]
    public async Task<IActionResult> RemoveMember(int chatId, int userId)
    {
        var member = await _context.Chatmembers
            .FirstOrDefaultAsync(cm =>
                cm.ChatId == chatId &&
                cm.UserId == userId);

        if (member == null)
        {
            return NotFound(new
            {
                message = "Пользователь не состоит в этом чате."
            });
        }

        _context.Chatmembers.Remove(member);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Пользователь удалён из чата."
        });
    }
}