using ASKART.API.Data;
using ASKART.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Services;

public sealed class AdminStatisticsService
{
    private readonly AskartContext _context;
    private readonly AdminChatNamesService names;
    public AdminStatisticsService(AskartContext context, AdminChatNamesService names)
    {
        _context = context;
        this.names = names;
    }

    public async Task<object?> GetStats()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var weekStart = today.AddDays(-6);
        var monthStart = today.AddDays(-29);

        // Пользователи
        var usersTotal = await _context.Users.CountAsync();
        var admins = await _context.Users.CountAsync(u => u.IsAdmin);
        var newUsers7 = await _context.Users.CountAsync(u => u.DataReg >= weekStart);
        var newUsers30 = await _context.Users.CountAsync(u => u.DataReg >= monthStart);
        var activeUsers7 = await _context.Messages
            .Where(m => m.SendTime >= weekStart)
            .Select(m => m.SenderId)
            .Distinct()
            .CountAsync();

        // Чаты
        var chatsTotal = await _context.Chats.CountAsync();
        var privateChats = await _context.Chats.CountAsync(c => c.TypeChat == "Личный");
        var personalGroups = await _context.Chats
            .CountAsync(c => c.TypeChat == "Группа" && c.TypeGroup == "Личная");
        var workGroups = await _context.Chats
            .CountAsync(c => c.TypeChat == "Группа" && c.TypeGroup == "Рабочая");

        // Сообщения
        var messagesTotal = await _context.Messages.CountAsync();
        var messagesToday = await _context.Messages.CountAsync(m => m.SendTime >= today);
        var messages7 = await _context.Messages.CountAsync(m => m.SendTime >= weekStart);
        var messagesEdited = await _context.Messages.CountAsync(m => m.EditTime != null);

        // Задачи
        var tasksTotal = await _context.Tasks.CountAsync();
        var tasksNew = await _context.Tasks.CountAsync(t => t.Status == "Новая");
        var tasksInProgress = await _context.Tasks.CountAsync(t => t.Status == "В работе");
        var tasksDone = await _context.Tasks.CountAsync(t => t.Status == "Выполнена");
        var tasksOverdue = await _context.Tasks
            .CountAsync(t => t.Deadline < now && t.Status != "Выполнена");

        // Сообщения по дням за последние 7 дней
        var daily = await _context.Messages
            .Where(m => m.SendTime >= weekStart)
            .GroupBy(m => m.SendTime.Date)
            .Select(g => new { date = g.Key, count = g.Count() })
            .ToListAsync();

        var messagesPerDay = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .Select(day => new
            {
                date = day,
                count = daily.FirstOrDefault(d => d.date.Date == day)?.count ?? 0
            })
            .ToList();

        // Топ-5 отправителей
        var topSenders = await _context.Messages
            .GroupBy(m => m.SenderId)
            .Select(g => new { userId = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        var senderIds = topSenders.Select(x => x.userId).ToList();

        var senderNames = await _context.Users
            .Where(u => senderIds.Contains(u.IdUser))
            .Select(u => new { u.IdUser, Name = u.Surname + " " + u.Name })
            .ToDictionaryAsync(u => u.IdUser, u => u.Name);

        var topUsers = topSenders.Select(x => new
        {
            name = senderNames.GetValueOrDefault(x.userId, "Неизвестно"),
            x.count
        }).ToList();

        // Топ-5 чатов по числу сообщений
        var topChatRows = await _context.Messages
            .GroupBy(m => m.ChatId)
            .Select(g => new { chatId = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        var topChatIds = topChatRows.Select(x => x.chatId).ToList();

        var topChatInfo = await _context.Chats
            .Where(c => topChatIds.Contains(c.IdChat))
            .Select(c => new { c.IdChat, c.Name, c.TypeChat })
            .ToListAsync();

        var topChatMembers = await names.LoadMembersAsync(topChatIds);

        var topChats = topChatRows.Select(x =>
        {
            var info = topChatInfo.FirstOrDefault(c => c.IdChat == x.chatId);

            return new
            {
                name = info == null
                    ? "Неизвестно"
                    : AdminChatNamesService.BuildChatName(
                        info.Name,
                        info.TypeChat,
                        topChatMembers.Where(m => m.ChatId == x.chatId).ToList()),
                x.count
            };
        }).ToList();

        return new
        {
            users = new
            {
                total = usersTotal,
                admins,
                new7Days = newUsers7,
                new30Days = newUsers30,
                active7Days = activeUsers7
            },
            chats = new
            {
                total = chatsTotal,
                privateChats,
                personalGroups,
                workGroups
            },
            messages = new
            {
                total = messagesTotal,
                today = messagesToday,
                last7Days = messages7,
                edited = messagesEdited
            },
            tasks = new
            {
                total = tasksTotal,
                created = tasksNew,
                inProgress = tasksInProgress,
                completed = tasksDone,
                overdue = tasksOverdue
            },
            messagesPerDay,
            topUsers,
            topChats
        };
    }

}
