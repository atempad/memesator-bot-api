using System.Text.RegularExpressions;
using App.Models.Services;
using App.Models.Services.Youtube;
using App.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace App.Services.Operations;

public class DownloadYoutubeVideoOperation(
    IOptions<AppSettings> appSettings) : DownloadMediaOperation
{
    public override async Task<MediaData> InvokeAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        
        string apiKey = appSettings.Value.YoutubeApiKey;
        string shortId = GetVideoIdFromUrl(urlString);
        using var metaDataRequest = CreateVideoMetaDataRequest(apiKey, shortId);
        using var metaDataResponse = await httpClient.SendAsync(metaDataRequest, cancellationToken);
        metaDataResponse.EnsureSuccessStatusCode();
        
        string metaDataJsonString = await metaDataResponse.Content.ReadAsStringAsync(cancellationToken);
        VideoMetaData? metaData = JsonConvert.DeserializeObject<VideoMetaData>(metaDataJsonString);
        VideoStreamingFormat? streamingFormat = metaData?.StreamingData.Formats.LastOrDefault();
        if (streamingFormat == null || string.IsNullOrWhiteSpace(streamingFormat.Url))
        {
            throw new InvalidOperationException("Failed to get media URL");
        }
        string streamUrl = streamingFormat.Url;
        
        const int chunkSize = Constants.Video.ChunkSize;
        int currentStart = 0;
        byte[] videoContentBytes = [];
        using (var videoContentStream = new MemoryStream())
        {
            bool endOfFile = false;
            while (!endOfFile)
            {
                httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(currentStart, currentStart + chunkSize - 1);
                using var chunkResponse = await httpClient.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
        }
        return new MediaData
        {
            MediaType = MediaType.Video, 
            MediaContentBytes = videoContentBytes
        };
    }

    public static HttpRequestMessage CreateVideoMetaDataRequest(string apiKey, string shortId)
    {
        string requestUrl = $"https://www.youtube.com/youtubei/v1/player?key={apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Content = new StringContent(
            $$"""
              {
                  "videoId": "{{shortId}}",
                  "context": {
                      "client": {
                          "clientName": "ANDROID_TESTSUITE",
                          "clientVersion": "1.9",
                          "androidSdkVersion": 30,
                          "hl": "en",
                          "gl": "US",
                          "utcOffsetMinutes": 0
                      }
                  }
              }
              """
        );
        request.Headers.Add(
            Constants.RequestHeaders.UserAgent,
            "com.google.android.youtube/17.36.4 (Linux; U; Android 12; GB) gzip"
        );
        return request;
    }
    
    public static string GetVideoIdFromUrl(string url)
    {
        const string pattern = @"(?:https?:\/\/)?(?:www\.)?youtube\.com\/(?:watch\?v=|shorts\/)([^&\/\?\n]+)";
        Match match = Regex.Match(url, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}