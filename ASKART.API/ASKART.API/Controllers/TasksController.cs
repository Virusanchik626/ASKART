
using ASKART.API.Data;
using ASKART.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskModel = ASKART.API.Models.Task;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AskartContext _context;

    public TasksController(AskartContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == dto.ChatId);

        if (!chatExists)
        {
            return NotFound(new { message = "Чат не найден." });
        }

        var creatorExists = await _context.Users
            .AnyAsync(u => u.IdUser == dto.CreatorId);

        if (!creatorExists)
        {
            return NotFound(new { message = "Создатель задачи не найден." });
        }

        if (dto.AssigneUserId.HasValue)
        {
            var assigneeExists = await _context.Users
                .AnyAsync(u => u.IdUser == dto.AssigneUserId.Value);

            if (!assigneeExists)
            {
                return NotFound(new { message = "Исполнитель не найден." });
            }
        }

        if (dto.Priority != "Низкий" &&
            dto.Priority != "Средний" &&
            dto.Priority != "Высокий")
        {
            return BadRequest(new
            {
                message = "Приоритет должен быть: Низкий, Средний или Высокий."
            });
        }

        var creatorIsMember = await _context.Chatmembers
            .AnyAsync(cm =>
                cm.ChatId == dto.ChatId &&
                cm.UserId == dto.CreatorId);

        if (!creatorIsMember)
        {
            return BadRequest(new
            {
                message = "Создатель не состоит в этом чате."
            });
        }

        if (dto.AssigneUserId.HasValue)
        {
            var assigneeIsMember = await _context.Chatmembers
                .AnyAsync(cm =>
                    cm.ChatId == dto.ChatId &&
                    cm.UserId == dto.AssigneUserId.Value);

            if (!assigneeIsMember)
            {
                return BadRequest(new
                {
                    message = "Исполнитель не состоит в этом чате."
                });
            }
        }

        var lastId = await _context.Tasks
            .Select(t => (int?)t.IdTask)
            .MaxAsync() ?? 0;

        var task = new TaskModel
        {
            IdTask = lastId + 1,
            ChatId = dto.ChatId,
            Name = dto.Name,
            Describe = dto.Describe,
            Status = "Новая",
            Priority = dto.Priority,
            Deadline = dto.Deadline ?? DateTime.Now.AddDays(7),
            CreatorId = dto.CreatorId,
            AssigneUserId = dto.AssigneUserId ?? 0
        };

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Задача создана.",
            taskId = task.IdTask,
            chatId = task.ChatId,
            name = task.Name,
            describe = task.Describe,
            status = task.Status,
            priority = task.Priority,
            deadline = task.Deadline,
            creatorId = task.CreatorId,
            assigneUserId = task.AssigneUserId
        });
    }

    [HttpGet("chat/{chatId}")]
    public async Task<IActionResult> GetChatTasks(int chatId)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return NotFound(new { message = "Чат не найден." });
        }

        var tasks = await _context.Tasks
            .Where(t => t.ChatId == chatId)
            .OrderBy(t => t.IdTask)
            .Select(t => new
            {
                taskId = t.IdTask,
                chatId = t.ChatId,
                name = t.Name,
                describe = t.Describe,
                status = t.Status,
                priority = t.Priority,
                deadline = t.Deadline,
                creatorId = t.CreatorId,
                assigneUserId = t.AssigneUserId
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTask(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => t.IdTask == taskId)
            .Select(t => new
            {
                taskId = t.IdTask,
                chatId = t.ChatId,
                name = t.Name,
                describe = t.Describe,
                status = t.Status,
                priority = t.Priority,
                deadline = t.Deadline,
                creatorId = t.CreatorId,
                assigneUserId = t.AssigneUserId
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        return Ok(task);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(
        int taskId,
        UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.IdTask == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        if (dto.Status != "Новая" &&
            dto.Status != "В работе" &&
            dto.Status != "Выполнена")
        {
            return BadRequest(new
            {
                message = "Статус должен быть: Новая, В работе или Выполнена."
            });
        }

        if (dto.Priority != "Низкий" &&
            dto.Priority != "Средний" &&
            dto.Priority != "Высокий")
        {
            return BadRequest(new
            {
                message = "Приоритет должен быть: Низкий, Средний или Высокий."
            });
        }

        task.Name = dto.Name;
        task.Describe = dto.Describe;
        task.Status = dto.Status;
        task.Priority = dto.Priority;

        if (dto.Deadline.HasValue)
        {
            task.Deadline = dto.Deadline.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Задача обновлена.",
            taskId = task.IdTask,
            name = task.Name,
            describe = task.Describe,
            status = task.Status,
            priority = task.Priority,
            deadline = task.Deadline
        });
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var task = await _context.Tasks
    .Include(t => t.Users)
    .FirstOrDefaultAsync(t => t.IdTask == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена." });
        }

        task.Users.Clear();

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Задача удалена.",
            taskId
        });
    }
}
