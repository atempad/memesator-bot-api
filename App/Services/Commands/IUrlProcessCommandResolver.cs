namespace App.Services.Commands;

public interface IUrlProcessCommandResolver
{
    IAsyncCommand<string>? Resolve(string url);
}