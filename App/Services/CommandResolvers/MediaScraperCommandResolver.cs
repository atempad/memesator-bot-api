using App.Services.Commands;

namespace App.Services.CommandResolvers;

public class MediaScraperCommandResolver(
    InstagramReelsScraperCommand instagramReelsScraperCommand) : IMediaScraperCommandResolver
{
    public IAsyncCommand<string>? Resolve(string url)
    {
        if (url.Contains("www.instagram.com"))
        {
            return instagramReelsScraperCommand.Setup(url);
        }
        return  null;
    }
}