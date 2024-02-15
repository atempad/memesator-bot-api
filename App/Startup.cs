using App.Middlewares;
using App.Repositories;
using App.Services.Commands;
using App.Services.Operations;
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
        services
            .AddHttpClient("Bot")
            .AddTypedClient<ITelegramBotApi, TelegramBotApi>();
        
        DbSettings dbSettings = ConfigureOptions<DbSettings>(services);
        services.AddSingleton(new CosmosClient(dbSettings.AccountEndpoint, dbSettings.AccountKey));
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        
        services.AddScoped<ITelegramBotOperationResolver, TelegramBotOperationResolver>();
        services.AddScoped<IUrlProcessCommandResolver, UrlProcessingCommandResolver>();
        
        services.AddHostedService<TelegramBotWebhookConfigurator>();
        
        services
            .AddControllers()
            .AddNewtonsoftJson();
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