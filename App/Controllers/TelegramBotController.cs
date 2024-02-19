using App.Attributes;
using App.Models.API;
using App.Models.DB;
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
        if (update.Message == null 
            || string.IsNullOrWhiteSpace(update.Message.Text)
            || update.Message.From == null
            || string.IsNullOrWhiteSpace(update.Message.From.Username))
        {
            return BadRequest("Invalid command");
        }
        TelegramUser sender = update.Message.From;
        TelegramChat chat = update.Message.Chat;
        var invoker = new BotUser
        {
            Id = sender.Username,
            Username = sender.Username,
            ChatId = chat.Id.ToString()
        };
        try
        {
            await botCommandRouter.RouteCommandAsync(invoker, update.Message.Text, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return Problem(ex.Message);
        }
        return Ok();
    }
}