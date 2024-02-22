namespace App.Services.Operations;

public interface IAsyncOperation<T>
{
    Task<T> InvokeAsync(CancellationToken cancellationToken = default);
}