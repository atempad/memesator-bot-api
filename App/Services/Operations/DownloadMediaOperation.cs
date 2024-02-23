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
}