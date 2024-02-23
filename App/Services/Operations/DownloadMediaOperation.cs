using App.Models.Services;

namespace App.Services.Operations;

public abstract class DownloadMediaOperation : IAsyncOperation<MediaData>
{ protected string urlString = string.Empty;
    
    public virtual DownloadMediaOperation Setup(string urlString)
    {
        this.urlString = urlString;
        return this;
    }

    public abstract Task<MediaData> InvokeAsync(CancellationToken cancellationToken = default);
    
    protected static async Task<byte[]> DownloadContentByChunksAsync(HttpClient httpClient, HttpRequestMessage request,
        int chunkSize, CancellationToken cancellationToken = default)
    {
        byte[] videoContentBytes = [];
        int currentStart = 0;
        using var videoContentStream = new MemoryStream();
        bool endOfFile = false;
        while (!endOfFile)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(currentStart, currentStart + chunkSize - 1);
            using var chunkResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            chunkResponse.EnsureSuccessStatusCode();
            if (chunkResponse.Content.Headers.ContentRange != null)
            {
                var chunkContentStream = await chunkResponse.Content.ReadAsStreamAsync(cancellationToken);
                await chunkContentStream.CopyToAsync(videoContentStream, cancellationToken);
                currentStart += chunkSize;
                var totalLength = chunkResponse.Content.Headers.ContentRange.Length;
                if (totalLength > Constants.Video.MaxSize)
                {
                    throw new InvalidOperationException("Media is too big");
                }
                if (currentStart >= totalLength)
                {
                    videoContentBytes = videoContentStream.ToArray();
                    endOfFile = true;
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to read media by chunks");
            }
        }
        return videoContentBytes;
    }
}