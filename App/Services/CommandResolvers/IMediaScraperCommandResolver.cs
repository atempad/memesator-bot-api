using App.Services.Commands;

namespace App.Services.CommandResolvers;

public interface IMediaScraperCommandResolver
{
    IAsyncCommand<string>? Resolve(string url);
}