using App.Models.API;
using App.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace App.Services.Telegram;

public class TelegramBotClientImpl : IBotClient
{
    private readonly TelegramBotClient _telegramBotClient;

    public TelegramBotClientImpl(IOptions<TelegramBotSettings> telegramBotOptions, HttpClient? httpClient = default)
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
    
    public async Task SendVideoAsync(string receiverId, byte[] buffer, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(buffer);
        await _telegramBotClient.SendVideoAsync(receiverId, InputFile.FromStream(stream),
            disableNotification: true,
            cancellationToken: cancellationToken);
    }
    
    public async Task SendVideoAsync(string[] receiversId, MediaInfo mediaInfo, CancellationToken cancellationToken = default)
    {
        if (mediaInfo.MediaType != MediaType.Video) throw new AggregateException();
        using var videoStream = new MemoryStream(mediaInfo.MediaContentBytes);
        InputFile videoInputFile = InputFile.FromStream(videoStream);
        InputFileId? videoInputFileId = null;
        using var thumbnailStream = new MemoryStream(mediaInfo.ThumbnailContentBytes);
        InputFile thumbrainFile = InputFile.FromStream(thumbnailStream);
        foreach (var receiverId in receiversId)
        {
            var message = await _telegramBotClient.SendVideoAsync(receiverId, 
                video: videoInputFileId ?? videoInputFile,
                thumbnail: thumbrainFile,
                width: mediaInfo.Width,
                height: mediaInfo.Height,
                duration: (int)mediaInfo.Duration,
                disableNotification: true,
                cancellationToken: cancellationToken);
            if (message.Video?.FileId != null && videoInputFileId == null)
            {
                videoInputFileId = InputFile.FromFileId(message.Video.FileId);
            }
        }
    }

    public async Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SetWebhookAsync(
            url: url,
            dropPendingUpdates: true,
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