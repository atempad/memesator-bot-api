using App.Models.Services;
using App.Repositories;
using App.Services.Operations;

namespace App.Services.Commands;

public class PostMediaCommand(
    IBotClient botClient,
    IScrapeMediaOperation scrapeMediaOperation) : IAsyncCommand<Media>
{
    private string invokerChatId = string.Empty;
    private string urlString = string.Empty;
    
    public PostMediaCommand Setup(string invokerChatId, string urlString)
    {
        this.invokerChatId = invokerChatId;
        this.urlString = urlString;
        return this;
    }
    
    public async Task<Media> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var media = await scrapeMediaOperation.Setup(urlString).InvokeAsync(cancellationToken);
        await botClient.SendVideoAsync(invokerChatId, media, cancellationToken);
        return media;
    }
}