namespace App.Services.Operations;

public interface IDownloadMediaOperationFactory
{
    DownloadMediaOperation Create(string mediaUrl);
}