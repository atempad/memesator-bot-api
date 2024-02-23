using App.Models.Services;

namespace App.Services.Operations;

public interface IScrapeMediaOperation : IAsyncOperation<Media>
{
    IScrapeMediaOperation Setup(string mediaUrl);
}