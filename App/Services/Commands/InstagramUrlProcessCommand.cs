namespace App.Services.Commands;

public class InstagramUrlProcessCommand(
    string _url) : IAsyncCommand<string>
{
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_url.Replace("www.", "dd")); // see https://ddinstagram.com for more details
    }
}