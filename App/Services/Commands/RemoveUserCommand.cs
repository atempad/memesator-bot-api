using App.Repositories;

namespace App.Services.Commands;

public class RemoveUserCommand(
    string _userId, 
    IUserRepository _userRepository) : IAsyncCommand<string>
{
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var resultMessage = $"User {_userId} successfully removed!";
        try
        {
            if (!await _userRepository.RemoveEntityAsync(_userId, cancellationToken))
            {
                resultMessage = $"User {_userId} doest not exist";
            }
        }
        catch (Exception ex)
        {
            resultMessage = "Something went wrong please try again later.\n" + ex.Message;
        }
        return await Task.FromResult(resultMessage);
    }
}