namespace App.Services.Operations;

public interface IAsyncOperation
{
    Task InvokeAsync(CancellationToken cancellationToken = default);
}