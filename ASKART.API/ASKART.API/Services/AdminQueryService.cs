using ASKART.API.Data;
using ASKART.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Services;

public sealed class AdminQueryService
{
    private readonly AskartContext _context;
    private readonly AdminChatNamesService names;
    public AdminQueryService(AskartContext context, AdminChatNamesService names)
    {
        _context = context;
        this.names = names;
    }

    public async Task<object?> GetUsers(
        string? search,
        int skip = 0,
        int take = 100)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 500);

        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();

            query = query.Where(u =>
                u.Surname.Contains(s) ||
                u.Name.Contains(s) ||
                (u.Lastname != null && u.Lastname.Contains(s)) ||
                u.Login.Contains(s) ||
                (u.Email != null && u.Email.Contains(s)) ||
                u.Number.Contains(s));
        }

        var total = await query.CountAsync();

        // Пароль и аватар намеренно не возвращаются
        var items = await query
            .OrderBy(u => u.IdUser)
            .Skip(skip)
            .Take(take)
            .Select(u => new
            {
                id = u.IdUser,
                surname = u.Surname,
                name = u.Name,
                lastname = u.Lastname,
                login = u.Login,
                email = u.Email,
                number = u.Number,
                registrationDate = u.DataReg,
                isAdmin = u.IsAdmin,
                chatsCount = u.Chatmembers.Count(),
                messagesCount = u.Messages.Count(),
                tasksCreatedCount = u.Tasks.Count()
            })
            .ToListAsync();

        return new { total, items };
    }

    public async Task<object?> GetChats(
        string? search,
        string? typeChat,
        string? typeGroup,
        int skip = 0,
        int take = 100)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 500);

        var query = _context.Chats.AsQueryable();

        if (!string.IsNullOrWhiteSpace(typeChat))
        {
            query = query.Where(c => c.TypeChat == typeChat);
        }

        if (!string.IsNullOrWhiteSpace(typeGroup))
        {
            query = query.Where(c => c.TypeGroup == typeGroup);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();

            // У личных чатов в поле name хранится имя собеседника,
            // поэтому ищем ещё и по именам участников
            query = query.Where(c =>
                c.Name.Contains(s) ||
                c.Chatmembers.Any(m =>
                    m.User.Surname.Contains(s) ||
                    m.User.Name.Contains(s)));
        }

        var total = await query.CountAsync();

        var page = await query
            .OrderByDescending(c => c.IdChat)
            .Skip(skip)
            .Take(take)
            .Select(c => new
            {
                id = c.IdChat,
                name = c.Name,
                typeChat = c.TypeChat,
                typeGroup = c.TypeGroup,
                creatorId = c.UserId,
                membersCount = c.Chatmembers.Count(),
                messagesCount = c.Messages.Count(),
                tasksCount = c.Tasks.Count(),
                lastMessageTime = c.Messages.Max(m => (DateTime?)m.SendTime)
            })
            .ToListAsync();

        var chatIds = page.Select(c => c.id).ToList();
        var members = await names.LoadMembersAsync(chatIds);

        var items = page.Select(c =>
        {
            var chatMembers = members.Where(m => m.ChatId == c.id).ToList();

            return new
            {
                c.id,
                name = AdminChatNamesService.BuildChatName(c.name, c.typeChat, chatMembers),
                c.typeChat,
                c.typeGroup,
                ownerName = chatMembers
                    .FirstOrDefault(m => m.UserId == c.creatorId)?.Name
                    ?? "Неизвестно",
                c.membersCount,
                c.messagesCount,
                c.tasksCount,
                c.lastMessageTime
            };
        }).ToList();

        return new { total, items };
    }

    public async Task<object?> GetChatMessages(
        int chatId,
        int take = 500)
    {
        take = Math.Clamp(take, 1, 1000);

        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return null;
        }

        var query = _context.Messages.Where(m => m.ChatId == chatId);

        var total = await query.CountAsync();

        // Берём последние сообщения, отдаём в хронологическом порядке
        var latest = await query
            .OrderByDescending(m => m.SendTime)
            .ThenByDescending(m => m.IdMessage)
            .Take(take)
            .Select(m => new
            {
                id = m.IdMessage,
                chatId = m.ChatId,
                senderId = m.SenderId,
                senderName = m.Sender.Surname + " " + m.Sender.Name,
                text = m.Text,
                replyId = m.ReplyId,
                sendTime = m.SendTime,
                editTime = m.EditTime
            })
            .ToListAsync();

        latest.Reverse();

        return new { total, items = latest };
    }

}
