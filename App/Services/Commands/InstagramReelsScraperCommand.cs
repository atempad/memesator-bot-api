using System.Net;
using App.Settings;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace App.Services.Commands;

public class InstagramReelsScraperCommand(
    IHostEnvironment environment) : IAsyncCommand<string>
{
    private string urlString = string.Empty;
    
    public InstagramReelsScraperCommand Setup(string urlString)
    {
        this.urlString = urlString;
        return this;
    }
    
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(urlString))
        {
            throw new ArgumentException("URL must not be empty", nameof(urlString));
        }
        
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox"]
        };
        if (environment.IsDevelopment())
        {
            await new BrowserFetcher().DownloadAsync();
        }
        await using var browser = await Puppeteer.LaunchAsync(launchOptions);
        
        var page = await browser.NewPageAsync();
        await page.GoToAsync(urlString);
        var waitForSelectorOptions = new WaitForSelectorOptions { Timeout = 10000 };
        await page.WaitForSelectorAsync("video", waitForSelectorOptions);
        
        var htmlDoc = new HtmlDocument();
        var htmlContent = await page.GetContentAsync();
        htmlDoc.LoadHtml(htmlContent);
        
        var videoNode = htmlDoc.DocumentNode.SelectSingleNode("//video");
        var encodedSrc = videoNode?.GetAttributeValue("src", string.Empty);
        var decodedSrc = WebUtility.HtmlDecode(encodedSrc); //
        
        return decodedSrc ?? throw new InvalidOperationException("Failed to get media URL");
    }
}