namespace App.Settings;

public class TelegramBotSettings
{
    public required string BotId { get; set; }
    public required string ApiSecret { get; set; }
    public string ApiToken => BotId + ":" + ApiSecret;
    public required string WebhookURL { get; set; }
}