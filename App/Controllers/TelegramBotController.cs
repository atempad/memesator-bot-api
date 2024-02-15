using App.Attributes;
using App.Models.API;
using App.Services.Operations;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramBotController(
    ITelegramBotOperationResolver botOperationResolver,
    ILogger<TelegramBotController> _logger) : ControllerBase
{
    [HttpPost("update")]
    [ValidateTelegramBotApiSecret]
    public async Task<IActionResult> Post(
        [FromBody] TelegramUpdate update,
        CancellationToken cancellationToken)
    {
        botOperationResolver.Setup(update);
        var command = botOperationResolver.Resolve();
        if (command is not null)
        {
            await command.InvokeAsync(cancellationToken);
        }
        return Ok();
    }
}