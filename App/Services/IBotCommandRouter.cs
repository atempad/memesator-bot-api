using App.Models.DB;

namespace App.Services;

public interface IBotCommandRouter
{
    Task RouteCommandAsync(BotUser invoker, string commandText, CancellationToken cancellationToken = default);
}