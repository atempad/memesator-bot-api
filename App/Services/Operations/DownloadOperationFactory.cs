using App.Settings;
using Microsoft.Extensions.Options;

namespace App.Services.Operations;

public class DownloadOperationFactory(
    IHostEnvironment environment,
    IOptions<AppSettings> appSettings) : IDownloadOperationFactory
{
    private delegate DownloadMediaOperation DownloadMediaOperationCreator();
    
    private readonly Dictionary<string, DownloadMediaOperationCreator> operationCreators = new()
    {
        { "instagram.com/reel/", () => new DownloadInstagramVideoOperation(environment) },
        { "youtube.com/", () => new DownloadYoutubeVideoOperation(appSettings) },
        { "tiktok.com/", () => new DownloadTikTokVideoOperation(environment) }
    };
    
    public DownloadMediaOperation Create(string mediaUrl)
    {
        foreach (var (pattern, downloadMediaOperationCreator) in operationCreators)
        {
            if (mediaUrl.Contains(pattern))
            {
                return downloadMediaOperationCreator.Invoke().Setup(mediaUrl);
            }
        }
        throw new ArgumentException("Unsupported media URL.", nameof(mediaUrl));
    }
}