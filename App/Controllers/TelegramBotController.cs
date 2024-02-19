using App.Attributes;
using App.Models.API;
using App.Models.API.Telegram;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramBotController(
    IBotCommandRouter botCommandRouter,
    ILogger<TelegramBotController> logger) : ControllerBase
{
    [HttpPost("update")]
    [ValidateTelegramBotApiSecret]
    public async Task<IActionResult> Post(
        [FromBody] TelegramUpdate update,
        CancellationToken cancellationToken)
    {
        if (update.Message != null
            && !string.IsNullOrWhiteSpace(update.Message.Text)
            && update.Message.From != null
            && !string.IsNullOrWhiteSpace(update.Message.From.Username))
        {
            var invoker = new InvokingContext(update.Message.From.Username, update.Message.Chat.Id.ToString());
            try
            {
                await botCommandRouter.RouteCommandAsync(invoker, update.Message.Text, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
        return Ok(); // return ok in any case to stop retry spamming
    }
}