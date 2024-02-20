namespace App.Services.Commands;

public class UrlProcessingCommandResolver : IUrlProcessCommandResolver
{
    public IAsyncCommand<string>? Resolve(string url)
    {
        if (url.Contains("www.instagram.com"))
        {
            return new InstagramUrlProcessCommand(url);
        }
        return null;
    }
}