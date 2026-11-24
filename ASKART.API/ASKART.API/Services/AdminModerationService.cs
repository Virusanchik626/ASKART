using ASKART.API.Data;
using ASKART.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Services;

public sealed class AdminModerationService
{
    private readonly AskartContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    public AdminModerationService(AskartContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<object?> DeleteMessage(int messageId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.IdMessage == messageId);

        if (message == null)
        {
            return null;
        }

        var chatId = message.ChatId;

        // Ответы на удаляемое сообщение остаются, но теряют ссылку
        var replies = await _context.Messages
            .Where(m => m.ReplyId == messageId)
            .ToListAsync();

        foreach (var reply in replies)
        {
            reply.ReplyId = null;
        }

        _context.Messages.Remove(message);

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"chat_{chatId}")
            .SendAsync("MessageDeleted", new { messageId, chatId });

        return new
        {
            message = "Сообщение удалено.",
            messageId
        };
    }

    public async Task<object?> DeleteChat(int chatId)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return null;
        }

        // В контексте включён EnableRetryOnFailure, поэтому
        // явную транзакцию нужно выполнять через стратегию выполнения
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // 1. Задачи чата вместе со связями исполнителей (task_assignees)
            var tasks = await _context.Tasks
                .Include(t => t.Users)
                .Where(t => t.ChatId == chatId)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.Users.Clear();
            }

            _context.Tasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            // 2. Остальные зависимые данные
            await _context.Messages
                .Where(m => m.ChatId == chatId)
                .ExecuteDeleteAsync();

            await _context.KanbanColumns
                .Where(k => k.ChatId == chatId)
                .ExecuteDeleteAsync();

            await _context.Chatmembers
                .Where(cm => cm.ChatId == chatId)
                .ExecuteDeleteAsync();

            // 3. Сам чат
            await _context.Chats
                .Where(c => c.IdChat == chatId)
                .ExecuteDeleteAsync();

            await transaction.CommitAsync();
        });

        await _hubContext.Clients
            .Group($"chat_{chatId}")
            .SendAsync("ChatDeleted", new { chatId });

        return new
        {
            message = "Чат удалён.",
            chatId
        };
    }

}
