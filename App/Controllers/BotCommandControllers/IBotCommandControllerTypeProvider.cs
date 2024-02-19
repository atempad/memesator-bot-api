namespace App.Controllers.BotCommandControllers;

public interface IBotCommandControllerTypeProvider
{
    IEnumerable<Type> GetAllTypes();
}