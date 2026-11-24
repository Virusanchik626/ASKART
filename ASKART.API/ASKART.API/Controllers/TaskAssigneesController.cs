
using ASKART.API.Data;
using ASKART.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskAssigneesController : ControllerBase
{
    private readonly AskartContext _context;

    public TaskAssigneesController(AskartContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddAssignee(
        [FromBody] TaskAssigneeDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.IdTask == dto.TaskId);

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUser == dto.UserId);

        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден." });
        }

        var isMember = await _context.Chatmembers
            .AnyAsync(cm =>
                cm.ChatId == task.ChatId &&
                cm.UserId == dto.UserId);

        if (!isMember)
        {
            return BadRequest(new
            {
                message = "Исполнитель не состоит в чате задачи."
            });
        }

        if (task.Users.Any(u => u.IdUser == dto.UserId))
        {
            return BadRequest(new
            {
                message = "Пользователь уже назначен на эту задачу."
            });
        }

        task.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Исполнитель назначен.",
            taskId = task.IdTask,
            userId = user.IdUser,
            surname = user.Surname,
            name = user.Name
        });
    }

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetAssignees(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.IdTask == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        var assignees = task.Users
            .Select(u => new
            {
                userId = u.IdUser,
                surname = u.Surname,
                name = u.Name,
                lastname = u.Lastname,
                login = u.Login
            })
            .ToList();

        return Ok(assignees);
    }

    [HttpDelete("{taskId}/{userId}")]
    public async Task<IActionResult> RemoveAssignee(
        int taskId,
        int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.IdTask == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        var user = task.Users
            .FirstOrDefault(u => u.IdUser == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не назначен на эту задачу."
            });
        }

        task.Users.Remove(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Исполнитель снят с задачи.",
            taskId,
            userId
        });
    }
}

public class TaskAssigneeDto
{
    public int TaskId { get; set; }

    public int UserId { get; set; }
}
