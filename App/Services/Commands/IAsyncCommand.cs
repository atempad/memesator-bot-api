namespace App.Services.Commands;

public interface IAsyncCommand
{
    Task InvokeAsync(CancellationToken cancellationToken = default);
}

public interface IAsyncCommand<T>
{
    Task<T> InvokeAsync(CancellationToken cancellationToken = default);
}