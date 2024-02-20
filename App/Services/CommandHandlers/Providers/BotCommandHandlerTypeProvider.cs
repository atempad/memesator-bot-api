using System.Reflection;
using App.Attributes;

namespace App.Services.CommandHandlers.Providers;

public class BotCommandHandlerTypeProvider : IBotCommandHandlerTypeProvider
{
    private readonly List<Type> types;

    public BotCommandHandlerTypeProvider()
    {
        types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(BotCommandHandler), inherit: true).Length != 0 
                           && type is { IsInterface: false, IsAbstract: false })
            .ToList();
    }
    
    public IEnumerable<Type> GetAllTypes()
    {
        return types;
    }
}