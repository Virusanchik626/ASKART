using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ASKART.API.Controllers;

// Сюда падают все необработанные ошибки сервера
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    [Route("error")]
    public IActionResult HandleError()
    {
        IExceptionHandlerFeature? feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

        string text = "Неизвестная ошибка сервера";

        if (feature != null)
        {
            Exception ex = feature.Error;
            text = ex.Message;

            // Самое важное обычно спрятано во внутренней ошибке (например, от MySQL)
            if (ex.InnerException != null)
            {
                text = text + " / " + ex.InnerException.Message;
            }
        }

        // Пишем в консоль докера, смотреть: docker compose logs -f api
        Console.WriteLine("ОШИБКА: " + text);

        return StatusCode(500, new { message = "Ошибка на сервере: " + text });
    }
}