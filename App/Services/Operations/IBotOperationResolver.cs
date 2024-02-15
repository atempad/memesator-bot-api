namespace App.Services.Operations;

public interface IBotOperationResolver
{
    public IAsyncOperation? Resolve();
}