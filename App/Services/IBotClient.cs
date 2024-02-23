using App.Models.Services;

namespace App.Services;

public interface IBotClient
{
    Task SendTextMessageAsync(string receiverId, string message, CancellationToken cancellationToken = default);
    Task SendVideoAsync(string receiverId, Media media, CancellationToken cancellationToken = default);
    Task SendVideoAsync(string[] receiversId, Media media, CancellationToken cancellationToken = default);
    Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken = default);
    Task<string> GetWebhookAsync(CancellationToken cancellationToken = default);
}