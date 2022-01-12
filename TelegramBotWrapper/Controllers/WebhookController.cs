using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Wrapper.Services;

namespace Telegram.Bot.Wrapper.Controllers;

public class WebhookController : ControllerBase
{
    private readonly IHandleUpdateService _handle;
    public WebhookController(IHandleUpdateService handle)
    {
        _handle = handle;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken ct)
    {
        await _handle.HandleAsync(update, ct);
        return Ok();
    }
}
