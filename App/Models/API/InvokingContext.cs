namespace App.Models.API;

public class InvokingContext(string userId, string chatId)
{
    public string UserId { get; } = userId;

    public string ChatId { get; } = chatId;
}