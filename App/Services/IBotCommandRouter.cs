using App.Models.API;

namespace App.Services;

public interface IBotCommandRouter
{
    Task RouteCommandAsync(InvokingContext invokingContext, string commandText, CancellationToken cancellationToken = default);
}