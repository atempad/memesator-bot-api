using App.Models.API;

namespace App.Services;

public interface IBotClient
{
    Task SendTextMessageAsync(string receiverId, string message, CancellationToken cancellationToken = default);
    Task SendVideoAsync(string receiverId, MediaInfo mediaInfo, CancellationToken cancellationToken = default);
    Task SendVideoAsync(string[] receiversId, MediaInfo mediaInfo, CancellationToken cancellationToken = default);
    Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken = default);
    Task<string> GetWebhookAsync(CancellationToken cancellationToken = default);
}