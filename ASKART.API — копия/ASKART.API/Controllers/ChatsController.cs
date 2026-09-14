using ASKART.API.Data;
using ASKART.API.DTOs;
using ASKART.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly AskartContext _context;

    public ChatsController(AskartContext context)
    {
        _context = context;
    }

    // Создание личного чата
    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateChat(CreatePrivateChatDto dto)
    {
        // Проверяем первого пользователя
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUser == dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        // Проверяем второго пользователя
        var otherUser = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUser == dto.OtherUserId);

        if (otherUser == null)
        {
            return NotFound(new
            {
                message = "Второй пользователь не найден."
            });
        }

        // Нельзя создать чат с самим собой
        if (dto.UserId == dto.OtherUserId)
        {
            return BadRequest(new
            {
                message = "Нельзя создать личный чат с самим собой."
            });
        }

        // Проверяем, существует ли уже личный чат между пользователями
        var existingChatId = await _context.Chats
    .Where(c => c.TypeChat == "Личный")
    .Where(c => _context.Chatmembers.Any(cm =>
        cm.ChatId == c.IdChat &&
        cm.UserId == dto.UserId))
    .Where(c => _context.Chatmembers.Any(cm =>
        cm.ChatId == c.IdChat &&
        cm.UserId == dto.OtherUserId))
    .Select(c => (int?)c.IdChat)
    .FirstOrDefaultAsync();

        if (existingChatId != null)
        {
            return Ok(new
            {
                message = "Личный чат уже существует.",
                chatId = existingChatId
            });
        }

        // Получаем новый ID чата
        var lastId = await _context.Chats
            .Select(c => (int?)c.IdChat)
            .MaxAsync() ?? 0;

        var chat = new Chat
        {
            IdChat = lastId + 1,
            Name = $"{otherUser.Surname} {otherUser.Name}",
            TypeChat = "Личный",
            TypeGroup = null,
            UserId = dto.UserId
        };

        _context.Chats.Add(chat);

        // Добавляем первого пользователя
        var firstMember = new Chatmember
        {
            ChatId = chat.IdChat,
            UserId = dto.UserId,
            Role = "Участник"
        };

        // Добавляем второго пользователя
        var secondMember = new Chatmember
        {
            ChatId = chat.IdChat,
            UserId = dto.OtherUserId,
            Role = "Участник"
        };

        _context.Chatmembers.Add(firstMember);
        _context.Chatmembers.Add(secondMember);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Личный чат создан.",
            chatId = chat.IdChat,
            name = chat.Name,
            typeChat = chat.TypeChat,
            members = new[]
            {
                new
                {
                    userId = firstMember.UserId,
                    role = firstMember.Role
                },
                new
                {
                    userId = secondMember.UserId,
                    role = secondMember.Role
                }
            }
        });
    }


    // Создание группы
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup(CreateGroupChatDto dto)
    {
        // Проверяем создателя
        var creatorExists = await _context.Users
            .AnyAsync(u => u.IdUser == dto.CreatorId);

        if (!creatorExists)
        {
            return NotFound(new
            {
                message = "Пользователь-создатель не найден."
            });
        }

        // Проверяем тип группы
        if (dto.TypeGroup != "Личная" && dto.TypeGroup != "Рабочая")
        {
            return BadRequest(new
            {
                message = "Тип группы должен быть 'Личная' или 'Рабочая'."
            });
        }

        // Получаем новый ID чата
        var lastId = await _context.Chats
            .Select(c => (int?)c.IdChat)
            .MaxAsync() ?? 0;

        var chat = new Chat
        {
            IdChat = lastId + 1,
            Name = dto.Name,
            TypeChat = "Группа",
            TypeGroup = dto.TypeGroup,
            UserId = dto.CreatorId
        };

        _context.Chats.Add(chat);

        // Сразу добавляем создателя группы
        var creatorMember = new Chatmember
        {
            ChatId = chat.IdChat,
            UserId = dto.CreatorId,
            Role = "Владелец"
        };

        _context.Chatmembers.Add(creatorMember);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Группа создана.",
            chatId = chat.IdChat,
            name = chat.Name,
            typeChat = chat.TypeChat,
            typeGroup = chat.TypeGroup,
            creator = new
            {
                userId = creatorMember.UserId,
                role = creatorMember.Role
            }
        });
    }
    // Получить все чаты пользователя
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserChats(int userId)
    {
        // Проверяем существование пользователя
        var userExists = await _context.Users
            .AnyAsync(u => u.IdUser == userId);

        if (!userExists)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        // Получаем все чаты, в которых состоит пользователь
        var memberships = await _context.Chatmembers
            .Where(cm => cm.UserId == userId)
            .Include(cm => cm.Chat)
            .ToListAsync();

        if (memberships.Count == 0)
        {
            return Ok(new List<object>());
        }

        // ID всех чатов пользователя
        var chatIds = memberships
            .Select(cm => cm.ChatId)
            .ToList();

        // Получаем всех участников этих чатов
        var allMembers = await _context.Chatmembers
            .Where(cm => chatIds.Contains(cm.ChatId))
            .Include(cm => cm.User)
            .ToListAsync();

        var chats = memberships.Select(cm =>
        {
            var chat = cm.Chat;

            // Для личного чата ищем второго пользователя
            if (chat.TypeChat == "Личный")
            {
                var otherMember = allMembers
                    .FirstOrDefault(member =>
                        member.ChatId == chat.IdChat &&
                        member.UserId != userId);

                return new
                {
                    chatId = chat.IdChat,
                    name = otherMember != null
                        ? $"{otherMember.User.Surname} {otherMember.User.Name}"
                        : "Неизвестный пользователь",
                    typeChat = chat.TypeChat,
                    typeGroup = chat.TypeGroup
                };
            }

            // Для группы используем название группы
            return new
            {
                chatId = chat.IdChat,
                name = chat.Name,
                typeChat = chat.TypeChat,
                typeGroup = chat.TypeGroup
            };
        }).ToList();

        return Ok(chats);
    }

}