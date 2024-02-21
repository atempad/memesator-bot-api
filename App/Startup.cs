using App.Extensions;
using App.Middlewares;
using App.Repositories;
using App.Services;
using App.Services.CommandHandlers.Providers;
using App.Services.CommandResolvers;
using App.Services.Commands;
using App.Services.Permissions;
using App.Services.Telegram;
using App.Settings;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Serialization;

namespace App;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureOptions<TelegramBotSettings>(services);
        ConfigureOptions<AppSettings>(services);
        
        services.AddHttpClient("BotHttpClient").AddTypedClient<IBotClient, TelegramBotClientImpl>();
        services.AddHostedService<TelegramBotWebhookConfigurator>();
        
        DbSettings dbSettings = ConfigureOptions<DbSettings>(services);
        services.AddScoped<CosmosClient>(_ => 
            new CosmosClient(dbSettings.AccountEndpoint, dbSettings.AccountKey));
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IPermissionManager, PermissionManager>();
        
        services.AddSingleton<IBotCommandRouter, BotCommandRouter>();
        var botCommandControllerTypeProvider = new BotCommandHandlerTypeProvider();
        services.AddSingleton<IBotCommandHandlerTypeProvider>(botCommandControllerTypeProvider);
        services.AddBotCommandHandlers(botCommandControllerTypeProvider);
        services.AddScoped<StartCommand>();
        services.AddScoped<StopCommand>();
        services.AddScoped<SetRoleCommand>();
        services.AddScoped<GetRoleCommand>();
        services.AddScoped<SubscribeUserCommand>();
        services.AddScoped<SubscribeChatCommand>();
        services.AddScoped<PostMediaCommand>();
        
        services.AddScoped<IMediaScraperCommandResolver, MediaScraperCommandResolver>();
        services.AddScoped<InstagramReelsScraperCommand>();
        services.AddScoped<DownloadVideoCommand>();

        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
        });
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