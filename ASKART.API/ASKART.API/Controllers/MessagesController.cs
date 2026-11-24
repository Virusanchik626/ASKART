
using ASKART.API.Data;
using ASKART.API.DTOs;
using ASKART.API.Models;
using ASKART.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly AskartContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(
        AskartContext context,
        IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(SendMessageDto dto)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == dto.ChatId);

        if (!chatExists)
        {
            return NotFound(new
            {
                message = "Чат не найден."
            });
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.IdUser == dto.SenderId);

        if (!userExists)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

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

        var sender = await _context.Users
            .FirstAsync(u => u.IdUser == dto.SenderId);

        var response = new
        {
            messageId = message.IdMessage,
            text = message.Text,
            chatId = message.ChatId,
            senderId = message.SenderId,
            senderName = $"{sender.Surname} {sender.Name}",
            replyId = message.ReplyId,
            sendTime = message.SendTime
        };

        await _hubContext.Clients
            .Group($"chat_{message.ChatId}")
            .SendAsync("ReceiveMessage", response);

        return Ok(response);
    }

    [HttpGet("chat/{chatId}")]
    public async Task<IActionResult> GetChatMessages(int chatId)
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

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(
        int messageId,
        [FromBody] string text)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.IdMessage == messageId);

        if (message == null)
        {
            return NotFound(new
            {
                message = "Сообщение не найдено."
            });
        }

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

    // Удаление сообщения
    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        // Find ищет по первичному ключу, лямбды не нужны
        var message = await _context.Messages.FindAsync(messageId);

        if (message == null)
        {
            return NotFound(new
            {
                message = "Сообщение не найдено."
            });
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Сообщение удалено.",
            messageId = messageId
        });
    }
}
