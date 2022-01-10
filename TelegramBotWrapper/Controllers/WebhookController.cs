using TelegramBotWrapper.Services;

namespace TelegramBotWrapper.Controllers;

public class WebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
    {
        await handleUpdateService.HandleAsync(update);
        return Ok();
    }
}
