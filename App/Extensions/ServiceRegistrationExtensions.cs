using App.Services.CommandHandlers.Providers;

namespace App.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddBotCommandHandlers(this IServiceCollection services, IBotCommandHandlerTypeProvider provider) 
    {
        foreach (var botCommandControllerType in provider.GetAllTypes())
        {
            services.AddScoped(botCommandControllerType);
        }
        return services;
    }
}