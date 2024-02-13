using System.Text;
using App.Attributes;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace App.Controllers;

[ApiController]
[Route("[controller]")]
public class BotController(ILogger<BotController> logger) : ControllerBase
{
    private readonly ILogger<BotController> _logger = logger;

    [HttpGet]
    public string Get()
    {
        return "v1";
    }
    
    [HttpPost]
    [ValidateTelegramBotApiSecret]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}