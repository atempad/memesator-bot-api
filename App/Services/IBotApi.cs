namespace App.Services;

public interface IBotApi
{
    Task SendTextMessageAsync(string receiverId, string message, CancellationToken cancellationToken = default);
    Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken = default);
    Task<string> GetWebhookAsync(CancellationToken cancellationToken = default);
}