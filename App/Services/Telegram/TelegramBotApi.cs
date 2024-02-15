using App.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace App.Services.Telegram;

public class TelegramBotApi : ITelegramBotApi
{
    private readonly TelegramBotClient _telegramBotClient;

    public TelegramBotApi(IOptions<TelegramBotSettings> telegramBotOptions, HttpClient? httpClient = default)
    {
        TelegramBotClientOptions options = new(telegramBotOptions.Value.ApiToken);
        _telegramBotClient = new TelegramBotClient(options, httpClient);
    }
    
    public async Task SendTextMessageAsync(string receiverId, string text, CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(receiverId, text,
            disableNotification: true,
            cancellationToken: cancellationToken);
    }

    public async Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SetWebhookAsync(
            url: url,
            dropPendingUpdates: false,
            allowedUpdates: [UpdateType.Message],
            secretToken: secretToken,
            cancellationToken: cancellationToken);
    }

    public async Task<string> GetWebhookAsync(CancellationToken cancellationToken = default)
    {
        var webhookInfo = await _telegramBotClient.GetWebhookInfoAsync(cancellationToken);
        return webhookInfo.Url;
    }
}