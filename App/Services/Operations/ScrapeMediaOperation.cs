using App.Models.Services;

namespace App.Services.Operations;

public class ScrapeMediaOperation(
    IDownloadMediaOperationFactory downloadMediaOperationFactory,
    GetVideoMetaAndThumbnailOperation getVideoMetaAndThumbnailOperation) : IScrapeMediaOperation
{
    private string mediaUrl = string.Empty;

    public IScrapeMediaOperation Setup(string mediaUrl)
    {
        this.mediaUrl = mediaUrl;
        return this;
    }

    public async Task<Media> InvokeAsync(CancellationToken cancellationToken = default)
    {
        MediaData mediaData = await downloadMediaOperationFactory
            .Create(mediaUrl)
            .InvokeAsync(cancellationToken);
        
        if (mediaData.MediaType == MediaType.Video)
        {
            return await getVideoMetaAndThumbnailOperation.Setup(mediaData)
                .InvokeAsync(cancellationToken);
        }
        throw new NotSupportedException();
    }
}