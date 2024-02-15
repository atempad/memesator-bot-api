using App.Models.API;

namespace App.Services.Operations;

public interface ITelegramBotOperationResolver : IBotOperationResolver
{
    void Setup(TelegramUpdate update);
}