using App.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace App.Services;

public class TelegramBotWebhookConfigurator(
    ILogger<TelegramBotWebhookConfigurator> _logger,
    IServiceProvider _serviceProvider,
    IOptions<TelegramBotSettings> _botOptions)
    : IHostedService
{
    private readonly TelegramBotSettings _botConfig = _botOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        _logger.LogInformation("Setting webhook: {WebhookURL}", _botConfig.WebhookURL);
        await botClient.SetWebhookAsync(
            url: _botConfig.WebhookURL,
            allowedUpdates: [UpdateType.Message],
            secretToken: _botConfig.ApiSecret,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}