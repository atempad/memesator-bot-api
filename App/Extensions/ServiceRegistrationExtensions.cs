using App.Controllers.BotCommandControllers;

namespace App.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddBotCommandControllers(this IServiceCollection services, IBotCommandControllerTypeProvider provider) 
    {
        foreach (var botCommandControllerType in provider.GetAllTypes())
        {
            services.AddScoped(botCommandControllerType);
        }
        return services;
    }
}