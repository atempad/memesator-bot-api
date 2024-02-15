using App.Settings;
using Microsoft.Extensions.Options;

namespace App.Services.Telegram;

public class TelegramBotWebhookConfigurator(
    ITelegramBotApi _botApi,
    IOptions<TelegramBotSettings> _botOptions,
    IHostEnvironment _environment,
    ILogger<TelegramBotWebhookConfigurator> _logger) : IHostedService
{
    private readonly TelegramBotSettings _botConfig = _botOptions.Value;
    private string _webhookUrlRestore = string.Empty;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting webhook: {WebhookURL}", _botConfig.WebhookURL);
        _webhookUrlRestore = await _botApi.GetWebhookAsync(cancellationToken);
        await _botApi.SetWebhookAsync(_botConfig.WebhookURL, _botConfig.ApiSecret, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("Restore webhook: {WebhookURL}", _webhookUrlRestore);
            await _botApi.SetWebhookAsync(_webhookUrlRestore, _botConfig.ApiSecret, cancellationToken);
        }
    }
}