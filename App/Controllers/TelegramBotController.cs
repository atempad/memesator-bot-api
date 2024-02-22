using App.Attributes;
using App.Models.API;
using App.Models.API.Telegram;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramBotController(
    IHostEnvironment environment,
    IBotClient botClient,
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
            var invokingContext = new InvokingContext
            {
                UserId = update.Message.From.Username,
                ChatId = update.Message.Chat.Id.ToString()
            };
            try
            {
                await botCommandRouter.RouteCommandAsync(invokingContext, update.Message.Text, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                if (environment.IsDevelopment())
                {
                    await botClient.SendTextMessageAsync(invokingContext.ChatId, ex.Message, cancellationToken);
                }
            }
        }
        return Ok(); // return ok in any case to stop retry spamming
    }
}