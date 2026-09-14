using ASKART.API.Data;
using ASKART.API.DTOs;
using ASKART.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly AskartContext _context;

    public MessagesController(AskartContext context)
    {
        _context = context;
    }

    // Отправить сообщение
    [HttpPost]
    public async Task<IActionResult> SendMessage(SendMessageDto dto)
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
            .AnyAsync(u => u.IdUser == dto.SenderId);

        if (!userExists)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        // Проверяем, что пользователь состоит в чате
        var isMember = await _context.Chatmembers
            .AnyAsync(cm =>
                cm.ChatId == dto.ChatId &&
                cm.UserId == dto.SenderId);

        if (!isMember)
        {
            return BadRequest(new
            {
                message = "Пользователь не состоит в этом чате."
            });
        }

        // Проверяем ответ на сообщение
        if (dto.ReplyId != null)
        {
            var replyExists = await _context.Messages
                .AnyAsync(m =>
                    m.IdMessage == dto.ReplyId &&
                    m.ChatId == dto.ChatId);

            if (!replyExists)
            {
                return BadRequest(new
                {
                    message = "Сообщение, на которое выполняется ответ, не найдено."
                });
            }
        }

        // Получаем новый ID сообщения
        var lastId = await _context.Messages
            .Select(m => (int?)m.IdMessage)
            .MaxAsync() ?? 0;

        var message = new Message
        {
            IdMessage = lastId + 1,
            Text = dto.Text,
            ChatId = dto.ChatId,
            SenderId = dto.SenderId,
            ReplyId = dto.ReplyId,
            SendTime = DateTime.Now
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        // Получаем данные отправителя
        var sender = await _context.Users
            .FirstAsync(u => u.IdUser == dto.SenderId);

        return Ok(new
        {
            messageId = message.IdMessage,
            text = message.Text,
            chatId = message.ChatId,
            senderId = message.SenderId,
            senderName = $"{sender.Surname} {sender.Name}",
            replyId = message.ReplyId,
            sendTime = message.SendTime
        });
    }
    // Получить сообщения чата
    [HttpGet("chat/{chatId}")]
    public async Task<IActionResult> GetChatMessages(int chatId)
    {
        // Проверяем существование чата
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return NotFound(new
            {
                message = "Чат не найден."
            });
        }

        // Получаем сообщения
        var messages = await _context.Messages
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SendTime)
            .Select(m => new
            {
                messageId = m.IdMessage,
                text = m.Text,
                chatId = m.ChatId,
                senderId = m.SenderId,

                senderName = $"{m.Sender.Surname} {m.Sender.Name}",

                replyId = m.ReplyId,

                sendTime = m.SendTime,
                editTime = m.EditTime
            })
            .ToListAsync();

        return Ok(messages);
    }
    // Редактировать сообщение
    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(
        int messageId,
        [FromBody] string text)
    {
        // Ищем сообщение
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.IdMessage == messageId);

        if (message == null)
        {
            return NotFound(new
            {
                message = "Сообщение не найдено."
            });
        }

        // Проверяем текст
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new
            {
                message = "Текст сообщения не может быть пустым."
            });
        }

        if (text.Length > 200)
        {
            return BadRequest(new
            {
                message = "Сообщение не должно превышать 200 символов."
            });
        }

        message.Text = text;
        message.EditTime = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            messageId = message.IdMessage,
            text = message.Text,
            editTime = message.EditTime
        });
    }
}