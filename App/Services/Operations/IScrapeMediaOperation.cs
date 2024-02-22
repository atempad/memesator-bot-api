using App.Models.API;

namespace App.Services.Operations;

public interface IScrapeMediaOperation : IAsyncOperation<MediaInfo?>
{
    IScrapeMediaOperation Setup(string mediaUrl);
}