using ASKART.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Filters;

/// <summary>
/// Пропускает запрос только если в заголовке X-Admin-Id указан
/// существующий пользователь с флагом is_admin = 1.
/// </summary>
public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "X-Admin-Id";

    public async System.Threading.Tasks.Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue(HeaderName, out var rawValue) ||
            !int.TryParse(rawValue.ToString(), out var adminId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Не указан идентификатор администратора."
            });
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<AskartContext>();

        var isAdmin = await db.Users
            .AnyAsync(u => u.IdUser == adminId && u.IsAdmin);

        if (!isAdmin)
        {
            context.Result = new ObjectResult(new
            {
                message = "Доступ запрещён: требуются права администратора."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}