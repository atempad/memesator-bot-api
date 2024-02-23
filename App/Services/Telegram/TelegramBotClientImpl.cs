using App.Models.Services;
using App.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace App.Services.Telegram;

public class TelegramBotClientImpl : IBotClient
{
    private readonly TelegramBotClient telegramBotClient;
    private readonly IHostEnvironment environment;

    public TelegramBotClientImpl(
        IOptions<TelegramBotSettings> telegramBotOptions,
        IHostEnvironment environment,
        HttpClient? httpClient = default)
    {
        TelegramBotClientOptions options = new(telegramBotOptions.Value.ApiToken);
        telegramBotClient = new TelegramBotClient(options, httpClient);
        this.environment = environment;
    }
    
    public async Task SendTextMessageAsync(string receiverId, string text, CancellationToken cancellationToken = default)
    {
        await telegramBotClient.SendTextMessageAsync(receiverId, text,
            disableNotification: true,
            cancellationToken: cancellationToken);
    }
    
    public async Task SendVideoAsync(string receiverId, Media media, CancellationToken cancellationToken = default)
    { 
        await SendVideoAsync([receiverId], media, cancellationToken);
    }
    
    public async Task SendVideoAsync(string[] receiversId, Media media, CancellationToken cancellationToken = default)
    {
        if (media.Data.MediaType != MediaType.Video)
        {
            throw new Exception();
        }
        
        using var videoStream = new MemoryStream(media.Data.MediaContentBytes);
        InputFile videoInputFile = InputFile.FromStream(videoStream);
        InputFileId? videoInputFileId = null;
        
        using var thumbnailStream = new MemoryStream(media.ThumbnailData.MediaContentBytes);
        InputFile thumbrainFile = InputFile.FromStream(thumbnailStream);
        
        foreach (var receiverId in receiversId)
        {
            var message = await telegramBotClient.SendVideoAsync(receiverId, 
                video: videoInputFileId ?? videoInputFile,
                thumbnail: thumbrainFile,
                width: media.Width,
                height: media.Height,
                duration: (int)media.Duration,
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
        await telegramBotClient.SetWebhookAsync(
            url: url,
            dropPendingUpdates: environment.IsDevelopment(),
            allowedUpdates: [UpdateType.Message],
            secretToken: secretToken,
            cancellationToken: cancellationToken);
    }

    public async Task<string> GetWebhookAsync(CancellationToken cancellationToken = default)
    {
        var webhookInfo = await telegramBotClient.GetWebhookInfoAsync(cancellationToken);
        return webhookInfo.Url;
    }
}