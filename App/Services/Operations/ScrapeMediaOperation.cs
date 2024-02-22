using App.Models.API;

namespace App.Services.Operations;

public class ScrapeMediaOperation(
    IServiceProvider serviceProvider) : IScrapeMediaOperation
{
    private string mediaUrl = string.Empty;

    private readonly Dictionary<string, Type> downloaderMap = new()
    {
        { "instagram.com/reel/", typeof(DownloadInstagramVideoOperation) },
        { "youtube.com/", typeof(DownloadYoutubeVideoOperation) },
    };
    
    public IScrapeMediaOperation Setup(string mediaUrl)
    {
        this.mediaUrl = mediaUrl;
        return this;
    }

    public async Task<MediaInfo?> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var scope = serviceProvider.CreateScope();
        MediaData? mediaData = null;
        foreach (var (pattern, downloaderType) in downloaderMap)
        {
            if (mediaUrl.Contains(pattern))
            {
                mediaData = await DownloadMedia(scope.ServiceProvider, downloaderType, mediaUrl, cancellationToken);
                break;
            }
        }
        if (mediaData?.MediaType == MediaType.Video)
        {
            return await scope.ServiceProvider
                .GetRequiredService<GetVideoMetaAndThumbnailOperation>()
                .Setup(mediaData)
                .InvokeAsync(cancellationToken);
        }
        return null;
    }

    private static async Task<MediaData> DownloadMedia(IServiceProvider serviceProvider,
        Type downloaderType,
        string mediaUrl, 
        CancellationToken cancellationToken = default)
    {
        var downloader = serviceProvider.GetRequiredService(downloaderType) as DownloadMediaOperation;
        return await downloader!.Setup(mediaUrl).InvokeAsync(cancellationToken);
    }
}