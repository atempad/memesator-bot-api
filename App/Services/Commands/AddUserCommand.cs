using App.Models.DB;
using App.Repositories;

namespace App.Services.Commands;

public class AddUserCommand(
    BotUser _userToAdd, 
    IUserRepository _userRepository) : IAsyncCommand<string>
{
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var resultMessage = $"User {_userToAdd.Id} successfully added!";
        try
        {
            if (!await _userRepository.AddEntityAsync(_userToAdd, cancellationToken))
            {
                resultMessage = $"User {_userToAdd.Id} already exists";
            }
        }
        catch (Exception ex)
        {
            resultMessage = "Something went wrong please try again later.\n" + ex.Message;
        }
        return await Task.FromResult(resultMessage);
    }
}