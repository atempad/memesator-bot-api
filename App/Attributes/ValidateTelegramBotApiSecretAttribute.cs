using App.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace App.Attributes;

/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateTelegramBotApiSecretAttribute() 
    : TypeFilterAttribute(typeof(ValidateTelegramBotFilter))
{
    private class ValidateTelegramBotFilter(IOptions<TelegramBotSettings> telegramBotSettings) : IActionFilter
    {
        private readonly string _secretToken = telegramBotSettings.Value.ApiSecret;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsValidRequest(context.HttpContext.Request))
            {
                context.Result = new ObjectResult($"\"{Constants.RequestHeaders.TelegramBotApiSecretToken}\" is invalid")
                {
                    StatusCode = 403
                };
            }
        }

        private bool IsValidRequest(HttpRequest request)
        {
            var isSecretTokenProvided = request.Headers.TryGetValue(Constants.RequestHeaders.TelegramBotApiSecretToken, 
                out var secretTokenHeader);
            return isSecretTokenProvided && string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
        }
    }
}