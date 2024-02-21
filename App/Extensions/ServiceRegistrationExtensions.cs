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
    
    public static T ConfigureOptions<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        IConfigurationSection settingsSection = configuration.GetSection(typeof(T).Name);
        T settings = settingsSection.Get<T>() ?? throw new InvalidOperationException(
            $"Configuration section '{typeof(T).Name}' is missing or cannot be bound to the type.");
        services.Configure<T>(settingsSection);
        return settings;
    }
}