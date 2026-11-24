using ASKART.API.Data;
using ASKART.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Services;

public sealed class AdminChatNamesService(AskartContext _context)
{
    public record MemberInfo(int ChatId, int UserId, string Name);

    public async Task<List<MemberInfo>> LoadMembersAsync(List<int> chatIds)
    {
        if (chatIds.Count == 0)
        {
            return new List<MemberInfo>();
        }

        return await _context.Chatmembers
            .Where(cm => chatIds.Contains(cm.ChatId))
            .Select(cm => new MemberInfo(
                cm.ChatId,
                cm.UserId,
                cm.User.Surname + " " + cm.User.Name))
            .ToListAsync();
    }

    // Для личного чата показываем обоих собеседников,
    // для групп — название группы
    public static string BuildChatName(
        string name,
        string typeChat,
        List<MemberInfo> members)
    {
        if (typeChat == "Личный" && members.Count > 0)
        {
            return string.Join(" ↔ ", members.Select(m => m.Name));
        }

        return name;
    }
}
