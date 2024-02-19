using App.Models.API;

namespace App.Services;

public interface IBotCommandRouter
{
    Task RouteCommandAsync(InvokingContext invoker, string commandText, CancellationToken cancellationToken = default);
}