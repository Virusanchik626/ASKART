using ASKART.API.Filters;
using ASKART.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/admin")]
[AdminOnly]
public class AdminController(
    AdminQueryService queries,
    AdminStatisticsService statistics,
    AdminModerationService moderation) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search,
        [FromQuery] int skip = 0, [FromQuery] int take = 100) =>
        Ok(await queries.GetUsers(search, skip, take));

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats([FromQuery] string? search,
        [FromQuery] string? typeChat, [FromQuery] string? typeGroup,
        [FromQuery] int skip = 0, [FromQuery] int take = 100) =>
        Ok(await queries.GetChats(search, typeChat, typeGroup, skip, take));

    [HttpGet("chats/{chatId}/messages")]
    public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] int take = 500)
    {
        var result = await queries.GetChatMessages(chatId, take);
        return result == null ? NotFound(new { message = "Чат не найден." }) : Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await statistics.GetStats());

    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var result = await moderation.DeleteMessage(messageId);
        return result == null ? NotFound(new { message = "Сообщение не найдено." }) : Ok(result);
    }

    [HttpDelete("chats/{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        var result = await moderation.DeleteChat(chatId);
        return result == null ? NotFound(new { message = "Чат не найден." }) : Ok(result);
    }
}
