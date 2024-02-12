namespace Services;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken stoppingToken);
}