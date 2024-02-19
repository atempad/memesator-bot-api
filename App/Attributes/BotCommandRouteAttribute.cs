namespace App.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BotCommandRouteAttribute : Attribute
{
    public BotCommandRouteAttribute()
    {
    }
    
    public BotCommandRouteAttribute(string route)
    {
        Route = route;
    }
    
    public string? Route { get; private set; }
}