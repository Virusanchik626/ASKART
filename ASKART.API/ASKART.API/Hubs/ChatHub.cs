
using ASKART.API.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Hubs;

public class ChatHub : Hub
{
    private readonly AskartContext _context;

    public ChatHub(AskartContext context)
    {
        _context = context;
    }

    public async Task JoinChat(int chatId, int userId)
    {
        var isMember = await _context.Chatmembers
            .AnyAsync(cm =>
                cm.ChatId == chatId &&
                cm.UserId == userId);

        if (!isMember)
        {
            throw new HubException(
                "Пользователь не состоит в этом чате.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"chat_{chatId}");
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"chat_{chatId}");
    }
}
