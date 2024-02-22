using App.Repositories;

namespace App.Services.Commands;

public class StopCommand(
    IUserRepository userRepository) : IAsyncCommand<bool>
{
    private string userId = string.Empty;
    
    public StopCommand Setup(string userId)
    {
        this.userId = userId;
        return this;
    }
    
    public async Task<bool> InvokeAsync(CancellationToken cancellationToken = default)
    {
        await userRepository.RemoveEntityAsync(userId, cancellationToken);
        return true;
    }
}