using App.Middlewares;
using App.Services;
using App.Settings;
using Microsoft.Azure.Cosmos;
using Telegram.Bot;

namespace App;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        TelegramBotSettings telegramBotSettings = ConfigureOptions<TelegramBotSettings>(services);
        DbSettings dbSettings = ConfigureOptions<DbSettings>(services);

        services
            .AddHttpClient(nameof(TelegramBotClient))
            .AddTypedClient<ITelegramBotClient>((httpClient, _) => 
            {
                TelegramBotClientOptions options = new(telegramBotSettings.ApiToken);
                return new TelegramBotClient(options, httpClient);
            });

        services
            .AddControllers()
            .AddNewtonsoftJson();
        
        services.AddHostedService<TelegramBotWebhookConfigurator>();
        services.AddScoped<UpdateHandler>();
        services.AddSingleton(new CosmosClient(dbSettings.AccountEndpoint, dbSettings.AccountKey));
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