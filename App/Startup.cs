using App.Controllers.BotCommandControllers;
using App.Extensions;
using App.Middlewares;
using App.Repositories;
using App.Services;
using App.Services.Commands;
using App.Services.Permissions;
using App.Services.Telegram;
using App.Settings;
using Microsoft.Azure.Cosmos;

namespace App;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureOptions<TelegramBotSettings>(services);
        
        services.AddHttpClient("BotHttpClient").AddTypedClient<IBotClient, TelegramBotClient>();
        services.AddHostedService<TelegramBotWebhookConfigurator>();
        
        DbSettings dbSettings = ConfigureOptions<DbSettings>(services);
        services.AddScoped<CosmosClient>(_ => new CosmosClient(dbSettings.AccountEndpoint, dbSettings.AccountKey));
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IPermissionManager, PermissionManager>();
        services.AddScoped<IUrlProcessCommandResolver, UrlProcessingCommandResolver>();
        
        services.AddSingleton<IBotCommandRouter, BotCommandRouter>();
        var botCommandControllerTypeProvider = new BotCommandControllerTypeProvider();
        services.AddSingleton<IBotCommandControllerTypeProvider>(botCommandControllerTypeProvider);
        
        services.AddBotCommandControllers(botCommandControllerTypeProvider);
        services.AddControllers().AddNewtonsoftJson();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
    
    private T ConfigureOptions<T>(IServiceCollection services) where T : class
    {
        IConfigurationSection settingsSection = Configuration.GetSection(typeof(T).Name);
        T settings = settingsSection.Get<T>() ?? throw new InvalidOperationException(
            $"Configuration section '{typeof(T).Name}' is missing or cannot be bound to the type.");
        services.Configure<T>(settingsSection);
        return settings;
    }
}