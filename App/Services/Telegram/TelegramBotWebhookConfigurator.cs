using App.Settings;
using Microsoft.Extensions.Options;

namespace App.Services.Telegram;

public class TelegramBotWebhookConfigurator(
    IBotClient botClient,
    IOptions<TelegramBotSettings> botOptions,
    IHostEnvironment environment,
    ILogger<TelegramBotWebhookConfigurator> logger) : IHostedService
{
    private readonly TelegramBotSettings botConfig = botOptions.Value;
    private string webhookUrlRestore = string.Empty;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting webhook: {WebhookURL}", botConfig.WebhookURL);
        webhookUrlRestore = await botClient.GetWebhookAsync(cancellationToken);
        await botClient.SetWebhookAsync(botConfig.WebhookURL, botConfig.ApiSecret, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation("Restore webhook: {WebhookURL}", webhookUrlRestore);
            await botClient.SetWebhookAsync(webhookUrlRestore, botConfig.ApiSecret, cancellationToken);
        }
    }
}