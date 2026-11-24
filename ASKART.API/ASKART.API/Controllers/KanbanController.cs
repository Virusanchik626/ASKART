
using ASKART.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KanbanController : ControllerBase
{
    private readonly AskartContext _context;

    public KanbanController(AskartContext context)
    {
        _context = context;
    }

    [HttpGet("chat/{chatId}")]
    public async Task<IActionResult> GetColumns(int chatId)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return NotFound(new { message = "Чат не найден." });
        }

        var columns = await _context.KanbanColumns
            .Where(c => c.ChatId == chatId)
            .OrderBy(c => c.Position)
            .Select(c => new
            {
                columnId = c.IdColumn,
                chatId = c.ChatId,
                name = c.Name,
                color = c.Color,
                position = c.Position
            })
            .ToListAsync();

        return Ok(columns);
    }

    [HttpPost("chat/{chatId}/initialize")]
    public async Task<IActionResult> InitializeBoard(int chatId)
    {
        var chatExists = await _context.Chats
            .AnyAsync(c => c.IdChat == chatId);

        if (!chatExists)
        {
            return NotFound(new { message = "Чат не найден." });
        }

        var defaults = new[]
        {
            new { Name = "Новая", Color = "#9E9E9E", Position = 1 },
            new { Name = "В работе", Color = "#5DD4FF", Position = 2 },
            new { Name = "Выполнена", Color = "#4CAF50", Position = 3 }
        };

        var existingColumns = await _context.KanbanColumns
            .Where(c => c.ChatId == chatId)
            .ToListAsync();

        var added = new List<object>();

        foreach (var item in defaults)
        {
            if (existingColumns.Any(c => c.Name == item.Name))
            {
                continue;
            }

            var lastId = existingColumns
                .Select(c => c.IdColumn)
                .DefaultIfEmpty(0)
                .Max();

            var column = new ASKART.API.Models.KanbanColumn
            {
                IdColumn = lastId + 1,
                ChatId = chatId,
                Name = item.Name,
                Color = item.Color,
                Position = item.Position
            };

            _context.KanbanColumns.Add(column);
            existingColumns.Add(column);

            added.Add(new
            {
                columnId = column.IdColumn,
                name = column.Name,
                color = column.Color,
                position = column.Position
            });
        }

        if (added.Count == 0)
        {
            return Ok(new
            {
                message = "Стандартные колонки уже существуют.",
                chatId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Колонки Kanban созданы.",
            chatId,
            columns = added
        });
    }

    [HttpPut("{chatId}/{columnId}")]
    public async Task<IActionResult> UpdateColumn(
        int chatId,
        int columnId,
        UpdateKanbanColumnDto dto)
    {
        var column = await _context.KanbanColumns
            .FirstOrDefaultAsync(c =>
                c.ChatId == chatId &&
                c.IdColumn == columnId);

        if (column == null)
        {
            return NotFound(new
            {
                message = "Колонка не найдена."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Color) ||
            dto.Color.Length > 20)
        {
            return BadRequest(new
            {
                message = "Укажите корректный цвет длиной до 20 символов."
            });
        }

        if (dto.Position < 1)
        {
            return BadRequest(new
            {
                message = "Позиция колонки должна быть не меньше 1."
            });
        }

        column.Color = dto.Color;
        column.Position = dto.Position;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Колонка обновлена.",
            columnId = column.IdColumn,
            chatId = column.ChatId,
            name = column.Name,
            color = column.Color,
            position = column.Position
        });
    }
}

public class UpdateKanbanColumnDto
{
    public string Color { get; set; } = null!;

    public int Position { get; set; }
}
