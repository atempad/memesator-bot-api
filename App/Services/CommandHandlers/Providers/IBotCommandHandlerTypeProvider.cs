namespace App.Services.CommandHandlers.Providers;

public interface IBotCommandHandlerTypeProvider
{
    IEnumerable<Type> GetAllTypes();
}