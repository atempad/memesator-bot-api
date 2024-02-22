using App.Models.API;
using App.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace App.Services.Operations;

public class DownloadYoutubeShortOperation(
    IOptions<AppSettings> appSettings) : DownloadMediaOperation
{
    public class MetaData
    {
        [JsonProperty("streamingData")]
        public required StreamingData StreamingData { get; set; }
    }

    public class StreamingData
    {
        [JsonProperty("formats")]
        public required List<Format> Formats { get; set; }
    }

    public class Format
    {
        [JsonProperty("url")]
        public required string Url { get; set; }
        
        [JsonProperty("contentLength")]
        public required string ContentLength { get; set; }
    }

    public override async Task<MediaData> InvokeAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        
        string apiKey = appSettings.Value.YoutubeApiKey;
        string shortId = GetShortIdFromUrl(urlString);
        using var metaDataRequest = CreateShortMetaDataRequest(apiKey, shortId);
        using var metaDataResponse = await httpClient.SendAsync(metaDataRequest, cancellationToken);
        metaDataResponse.EnsureSuccessStatusCode();
        
        string metaDataContent = await metaDataResponse.Content.ReadAsStringAsync(cancellationToken);
        MetaData? metaData = JsonConvert.DeserializeObject<MetaData>(metaDataContent);
        Format? streamMetaData = metaData?.StreamingData.Formats.LastOrDefault();
        if (streamMetaData == null || string.IsNullOrWhiteSpace(streamMetaData.Url))
        {
            throw new InvalidOperationException("Failed to get media URL");
        }
        string streamUrl = streamMetaData.Url;
        int streamContentLength = Convert.ToInt32(streamMetaData.ContentLength);
        if (streamContentLength > Constants.Video.MaxSize)
        {
            throw new InvalidOperationException("Media is too big");
        }
        
        const int chunkSize = Constants.Video.ChunkSize;
        int currentStart = 0;
        byte[] videoContentBytes = [];
        await using (var videoContentStream = new MemoryStream())
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
                    if (currentStart >= chunkResponse.Content.Headers.ContentRange.Length)
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

    public static HttpRequestMessage CreateShortMetaDataRequest(string apiKey, string shortId)
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
    
    public static string GetShortIdFromUrl(string url)
    {
        Uri uri = new Uri(url);
        string path = uri.AbsolutePath;
        const string shortsPath = "/shorts/";
        int index = path.IndexOf(shortsPath, StringComparison.OrdinalIgnoreCase);
        return index != -1 ? path[(index + shortsPath.Length)..] : string.Empty;
    }
}