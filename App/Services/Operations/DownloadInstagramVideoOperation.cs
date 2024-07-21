using System.Net;
using App.Models.Services;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace App.Services.Operations;

public class DownloadInstagramVideoOperation(
    IHostEnvironment environment) : DownloadMediaOperation
{
    public override async Task<MediaData> InvokeAsync(CancellationToken cancellationToken = default)
    {
        await using var browser = await GetBrowser(environment.IsDevelopment());
        var page = await browser.NewPageAsync();
        var deviceOptions = Puppeteer.Devices.GetValueOrDefault(PuppeteerSharp.Mobile.DeviceDescriptorName.IPhone5);
        await page.EmulateAsync(deviceOptions);
        await page.GoToAsync(urlString, new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle0] // Wait until there are no more network requests for at least 500ms
        });
        var waitForSelectorOptions = new WaitForSelectorOptions { Timeout = 10000 };
        await page.WaitForSelectorAsync("video", waitForSelectorOptions);
        
        var htmlDoc = new HtmlDocument();
        var htmlContent = await page.GetContentAsync();
        htmlDoc.LoadHtml(htmlContent);
        
        var videoNode = htmlDoc.DocumentNode.SelectSingleNode("//video");
        var encodedSrc = videoNode?.GetAttributeValue("src", string.Empty);
        var decodedSrc = WebUtility.HtmlDecode(encodedSrc);

        if (string.IsNullOrWhiteSpace(decodedSrc))
        {
            throw new InvalidOperationException("Failed to get media URL");
        }
        
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(decodedSrc, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var mediaContentBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        
        return new MediaData
        {
            MediaType = MediaType.Video,
            MediaContentBytes = mediaContentBytes
        };
    }
}