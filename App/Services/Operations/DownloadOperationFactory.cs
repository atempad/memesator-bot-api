using App.Settings;
using Microsoft.Extensions.Options;

namespace App.Services.Operations;

public class DownloadOperationFactory(
    IHostEnvironment environment,
    IOptions<AppSettings> appSettings) : IDownloadOperationFactory
{
    public DownloadMediaOperation Create(string mediaUrl)
    {
        if (mediaUrl.Contains("instagram.com/reel/"))
        {
            return new DownloadInstagramVideoOperation(environment).Setup(mediaUrl);
        }
        if (mediaUrl.Contains("youtube.com/"))
        {
            return new DownloadYoutubeVideoOperation(appSettings).Setup(mediaUrl);;
        }
        throw new ArgumentException("Unsupported media URL.", nameof(mediaUrl));
    }
}