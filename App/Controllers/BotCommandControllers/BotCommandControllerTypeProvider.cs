using System.Reflection;
using App.Attributes;

namespace App.Controllers.BotCommandControllers;

public class BotCommandControllerTypeProvider : IBotCommandControllerTypeProvider
{
    private readonly List<Type> types;

    public BotCommandControllerTypeProvider()
    {
        types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(BotCommandController), inherit: true).Length != 0 
                           && type is { IsInterface: false, IsAbstract: false })
            .ToList();
    }
    
    public IEnumerable<Type> GetAllTypes()
    {
        return types;
    }
}