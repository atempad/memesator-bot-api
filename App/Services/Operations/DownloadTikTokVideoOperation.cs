using System.Net;
using App.Models.Services;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace App.Services.Operations;

public class DownloadTikTokVideoOperation(
    IHostEnvironment environment) : DownloadMediaOperation
{
    public override async Task<MediaData> InvokeAsync(CancellationToken cancellationToken = default)
    {
        await using var browser = await GetBrowser(environment.IsDevelopment());

        var page = await browser.NewPageAsync();
        await page.GoToAsync(urlString);
        var waitForSelectorOptions = new WaitForSelectorOptions { Timeout = 10000 };
        await page.WaitForSelectorAsync("video", waitForSelectorOptions);
        
        var cookies = await page.GetCookiesAsync();
        var htmlContent = await page.GetContentAsync();
        
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        var videoNode = htmlDoc.DocumentNode.SelectSingleNode("//video");
        var encodedSrc = videoNode?.GetAttributeValue("src", string.Empty);
        var decodedSrc = WebUtility.HtmlDecode(encodedSrc);

        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.UseCookies = false;
        using var httpClient = new HttpClient(httpClientHandler);
        
        var videoContentRequest = new HttpRequestMessage(HttpMethod.Get, decodedSrc);
        videoContentRequest.Headers.Referrer = new Uri("https://www.tiktok.com/");
        videoContentRequest.Headers.Add(Constants.RequestHeaders.Cookie,
            string.Join("; ", cookies.Select(p => p.Name + "=" + p.Value)));
        
        var videoContentBytes = await DownloadContentByChunksAsync(httpClient, videoContentRequest, 
            Constants.Video.ChunkSize, cancellationToken);
        
        return new MediaData
        {
            MediaType = MediaType.Video, 
            MediaContentBytes = videoContentBytes
        };
    }
}