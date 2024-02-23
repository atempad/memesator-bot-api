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
    private Media _media;

    public GetVideoMetaAndThumbnailOperation Setup(MediaData mediaData)
    {
        _media.Data = mediaData;
        return this;
    }
    
    public async Task<Media> InvokeAsync(CancellationToken cancellationToken = default)
    {
        InputFile? videoFile = null;
        string tempVideoPath = string.Empty;
        OutputFile? thumbnailFile = null;
        string thumbnailPath = string.Empty;
        try
        {
            var ffmpeg = new Engine(appSettings.Value.FFMpegPath);
            ffmpeg.Data += OnData;
            tempVideoPath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempVideoPath, _media.Data.MediaContentBytes, cancellationToken);
            videoFile = new InputFile(tempVideoPath);
            var metaData = await ffmpeg.GetMetaDataAsync(videoFile, cancellationToken);

            thumbnailPath = Path.ChangeExtension(tempVideoPath, ".jpg");
            thumbnailFile = new OutputFile(thumbnailPath);
            await ffmpeg.GetThumbnailAsync(videoFile, thumbnailFile, cancellationToken);
            
            _media.Duration = metaData.Duration.TotalSeconds;
            var frameSize = metaData.VideoData?.FrameSize.Split('x');
            if (frameSize is { Length: 2 })
            {
                _media.Width = Convert.ToInt32(frameSize[0]);
                _media.Height = Convert.ToInt32(frameSize[1]);
            }
            else
            {
                _media.Width = null;
                _media.Height = null;
            }
            _media.ThumbnailData = new MediaData
            {
                MediaType = MediaType.Image,
                MediaContentBytes = await File.ReadAllBytesAsync(thumbnailPath, cancellationToken)
            };
            
            File.Delete(tempVideoPath);
            File.Delete(thumbnailPath);
        }
        catch
        {
            if (videoFile != null) File.Delete(tempVideoPath);
            if (thumbnailFile != null) File.Delete(thumbnailPath);
            throw;
        }
        return _media;
    }

    private void OnData(object? sender, ConversionDataEventArgs e)
    {
        if (environment.IsDevelopment())
        {
            Console.WriteLine(e.Data);
        }
    }
}