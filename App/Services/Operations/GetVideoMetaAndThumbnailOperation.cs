using App.Models.Services;
using App.Settings;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using Microsoft.Extensions.Options;

namespace App.Services.Operations;

public class GetVideoMetaAndThumbnailOperation(
    IHostEnvironment environment,
    IOptions<AppSettings> appSettings) : IAsyncOperation<Media>
{
    private Media media;

    public GetVideoMetaAndThumbnailOperation Setup(MediaData mediaData)
    {
        media.Data = mediaData;
        return this;
    }
    
    public async Task<Media> InvokeAsync(CancellationToken cancellationToken = default)
    {
        InputFile? videoFile = null;
        string videoPath = string.Empty;
        OutputFile? thumbnailFile = null;
        string thumbnailPath = string.Empty;
        try
        {
            var ffmpeg = new Engine(appSettings.Value.FFMpegPath);
            ffmpeg.Data += OnData;
            
            videoPath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(videoPath, media.Data.MediaContentBytes, cancellationToken);
            videoFile = new InputFile(videoPath);
            var metaData = await ffmpeg.GetMetaDataAsync(videoFile, cancellationToken);

            thumbnailPath = Path.ChangeExtension(videoPath, ".jpg");
            thumbnailFile = new OutputFile(thumbnailPath);
            await ffmpeg.GetThumbnailAsync(videoFile, thumbnailFile, cancellationToken);
            
            media.Duration = metaData.Duration.TotalSeconds;
            var frameSize = metaData.VideoData?.FrameSize.Split('x');
            if (frameSize is { Length: 2 })
            {
                media.Width = Convert.ToInt32(frameSize[0]);
                media.Height = Convert.ToInt32(frameSize[1]);
            }
            else
            {
                media.Width = null;
                media.Height = null;
            }
            media.ThumbnailData = new MediaData
            {
                MediaType = MediaType.Image,
                MediaContentBytes = await File.ReadAllBytesAsync(thumbnailPath, cancellationToken)
            };
        }
        finally
        {
            if (videoFile != null) File.Delete(videoPath);
            if (thumbnailFile != null) File.Delete(thumbnailPath);
        }
        return media;
    }

    private void OnData(object? sender, ConversionDataEventArgs e)
    {
        if (environment.IsDevelopment())
        {
            Console.WriteLine(e.Data);
        }
    }
}