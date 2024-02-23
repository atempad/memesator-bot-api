namespace App.Services.Operations;

public interface IDownloadOperationFactory
{
    DownloadMediaOperation Create(string mediaUrl);
}