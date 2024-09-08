using App.Models.DB;
using App.Repositories;
using App.Services.Permissions;

namespace App.Services.Commands;

public class StartCommand(
    IUserRepository userRepository) : IAsyncCommand<bool>
{
    private string userId = string.Empty;
    private string userChatId = string.Empty;
    
    public StartCommand Setup(string userId, string userChatId)
    {
        this.userId = userId;
        this.userChatId = userChatId;
        return this;
    }
    
    public async Task<bool> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var hasAnyEntity = await userRepository.HasAnyEntityAsync(cancellationToken: cancellationToken);
        var newUser = new ServiceUser
        {
            Id = userId,
            ChatId = userChatId,
            Role = hasAnyEntity ? Role.User : Role.Admin
        };
        await userRepository.AddEntityAsync(newUser, cancellationToken);
        return true;
    }
}