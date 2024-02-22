using App.Models.API;

namespace App.Services.Operations;

public class ScrapeMediaOperation(
    IServiceProvider serviceProvider) : IScrapeMediaOperation
{
    private string mediaUrl = string.Empty;
    
    public IScrapeMediaOperation Setup(string mediaUrl)
    {
        this.mediaUrl = mediaUrl;
        return this;
    }

    public async Task<MediaInfo?> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var scope = serviceProvider.CreateScope();
        if (mediaUrl.Contains("www.instagram.com"))
        {
            var mediaData = await scope.ServiceProvider
                .GetRequiredService<DownloadInstagramReelsOperation>()
                .Setup(mediaUrl)
                .InvokeAsync(cancellationToken);
            return await scope.ServiceProvider
                .GetRequiredService<GetVideoMetaAndThumbnailOperation>()
                .Setup(mediaData)
                .InvokeAsync(cancellationToken);
        }
        return null;
    }
}