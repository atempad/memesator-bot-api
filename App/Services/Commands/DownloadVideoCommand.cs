using App.Models.API;
using App.Settings;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using Microsoft.Extensions.Options;

namespace App.Services.Commands;

public class DownloadVideoCommand(
    IHostEnvironment environment,
    IOptions<AppSettings> appSettings) : IAsyncCommand<MediaInfo>
{
    private string videoUrlString = string.Empty;

    public DownloadVideoCommand Setup(string videoUrlString)
    {
        this.videoUrlString = videoUrlString;
        return this;
    }
    
    public async Task<MediaInfo> InvokeAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(videoUrlString, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        MediaInfo mediaInfo;
        InputFile? videoFile = null;
        string tempVideoPath = string.Empty;
        OutputFile? thumbnailFile = null;
        string thumbnailPath = string.Empty;
        try
        {
            mediaInfo.MediaType = MediaType.Video;
            mediaInfo.MediaContentBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var ffmpeg = new Engine(appSettings.Value.FFMpegPath);
            ffmpeg.Data += OnData;
            tempVideoPath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempVideoPath, mediaInfo.MediaContentBytes, cancellationToken);
            videoFile = new InputFile(tempVideoPath);
            var metaData = await ffmpeg.GetMetaDataAsync(videoFile, cancellationToken);

            thumbnailPath = Path.ChangeExtension(tempVideoPath, ".jpg");
            thumbnailFile = new OutputFile(thumbnailPath);
            await ffmpeg.GetThumbnailAsync(videoFile, thumbnailFile, cancellationToken);
            
            mediaInfo.Duration = metaData.Duration.TotalSeconds;
            var frameSize = metaData.VideoData?.FrameSize.Split('x');
            if (frameSize is { Length: 2 })
            {
                mediaInfo.Width = Convert.ToInt32(frameSize[0]);
                mediaInfo.Height = Convert.ToInt32(frameSize[1]);
            }
            else
            {
                mediaInfo.Width = null;
                mediaInfo.Height = null;
            }
            mediaInfo.ThumbnailContentBytes = await File.ReadAllBytesAsync(thumbnailPath, cancellationToken);
            
            File.Delete(tempVideoPath);
            File.Delete(thumbnailPath);
        }
        catch
        {
            if (videoFile != null) File.Delete(tempVideoPath);
            if (thumbnailFile != null) File.Delete(thumbnailPath);
            throw;
        }
        return mediaInfo;
    }

    private void OnData(object? sender, ConversionDataEventArgs e)
    {
        if (environment.IsDevelopment())
        {
            Console.WriteLine(e.Data);
        }
    }
}