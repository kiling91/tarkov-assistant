using Microsoft.AspNetCore.Mvc;
using Tarkov.Assistant.Telegram.Bot.Services;
using Telegram.Bot.Types;

namespace Tarkov.Assistant.Telegram.Bot.Controllers;

public class WebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}
