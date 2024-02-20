using System.Reflection;
using App.Attributes;
using App.Controllers.BotCommandControllers;
using App.Models.API;
using App.Repositories;
using App.Services.Permissions;

namespace App.Services;

public class BotCommandRouter : IBotCommandRouter
{
    public class InvokeContext(Type controllerType, MethodInfo method, Permission requiredPermissions)
    {
        public readonly Type ControllerType = controllerType;
        public readonly MethodInfo Method = method;
        public readonly ParameterInfo[] MethodParameters = method.GetParameters();
        public readonly Permission RequiredPermissions = requiredPermissions;
    }

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<BotCommandRouter> logger;
    private readonly Dictionary<string, InvokeContext> commandHandlers = new();
    private readonly List<string> commandKeys = [];

    public BotCommandRouter(
        IBotCommandControllerTypeProvider botCommandControllerTypeProvider, 
        IServiceProvider serviceProvider,
        ILogger<BotCommandRouter> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        
        foreach (var controllerType in botCommandControllerTypeProvider.GetAllTypes())
        {
            var classRoute = controllerType
                .GetCustomAttribute<BotCommandRouteAttribute>()?.Route?.ToLower() ?? "";

            var methods = controllerType
                .GetMethods()
                .Where(m => m.GetCustomAttributes<BotCommandRouteAttribute>().Any());
            
            foreach (var method in methods)
            {
                var methodRequiredPermissions = method
                    .GetCustomAttributes<RequiredPermissionAttribute>()
                    .Select(a => a.RequiredPermission)
                    .Aggregate(Permission.None, (result, permission) => result | permission);
                
                var methodRoute = method
                    .GetCustomAttribute<BotCommandRouteAttribute>()?.Route?
                    .ToLower() ?? "";
                
                var fullRoute = string.IsNullOrEmpty(classRoute) ? methodRoute : $"{classRoute} {methodRoute}";
                fullRoute = fullRoute.Trim();
                
                commandHandlers[fullRoute] = new InvokeContext(controllerType, method, methodRequiredPermissions);
                commandKeys.Add(fullRoute);
            }
        }
        
        commandKeys = commandKeys
            .OrderByDescending(c => c.Split(' ').Length)
            .ThenByDescending(c => c.Length)
            .ToList();
        
        logger.LogInformation($"Available commands: {string.Join(", ", commandKeys)}");
    }

    public async Task RouteCommandAsync(InvokingContext invokingContext, string commandText, 
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var invokerUser = await scope.ServiceProvider.GetRequiredService<IUserRepository>()
            .GetEntityAsync(invokingContext.UserId, cancellationToken);
        
        bool hasFound = false;
        foreach (var commandKey in commandKeys)
        {
            if (commandText.StartsWith(commandKey))
            {
                if (commandHandlers.TryGetValue(commandKey, out var invokeContext))
                {
                    var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();
                    if (invokeContext.RequiredPermissions != Permission.None
                        && !permissionManager.CheckPermissions(invokerUser?.Role, invokeContext.RequiredPermissions))
                    {
                        throw new UnauthorizedAccessException($"User {invokingContext.UserId} doesn't have required permissions" +
                                                              $"{string.Join(", ", invokeContext.RequiredPermissions)}");
                    }

                    var arguments = commandText[commandKey.Length..].Trim().Split(' ');
                    var methodParams = new List<object>();
                    int paramIndex = 0;
                    foreach (var param in invokeContext.MethodParameters)
                    {
                        if (param.ParameterType == typeof(InvokingContext))
                        {
                            methodParams.Add(invokingContext);
                        }
                        else if (param.ParameterType == typeof(CancellationToken))
                        {
                            methodParams.Add(cancellationToken);
                        }
                        else if (paramIndex < arguments.Length)
                        {
                            var argumentIndex = paramIndex++;
                            try
                            {
                                var argument = Convert.ChangeType(arguments[argumentIndex], param.ParameterType);
                                methodParams.Add(argument);
                            }
                            catch
                            {
                                throw new ArgumentException($"Failed to cast argument '{arguments[argumentIndex]}' " +
                                                            $"to type '{param.ParameterType.Name}'");
                            }
                        }
                        else if (param.HasDefaultValue)
                        {
                            methodParams.Add(param.DefaultValue!);
                        }
                        else
                        {
                            throw new ArgumentException($"Missing argument for parameter '{param.Name}'");
                        }
                    }
                    
                    var controller = scope.ServiceProvider.GetService(invokeContext.ControllerType);
                    if (controller == null)
                    {
                        throw new Exception($"Controller of type {invokeContext.ControllerType.Name} is not found");
                    }
                    
                    logger.LogInformation($"Invoking {invokeContext.ControllerType.Name}:{invokeContext.Method.Name}/n" +
                                          $"({string.Join(',', invokeContext.MethodParameters.Select((x, i) => x.Name + "=" + methodParams[i]))})");
                    
                    if (typeof(Task).IsAssignableFrom(invokeContext.Method.ReturnType))
                    {
                        var task = (Task)invokeContext.Method.Invoke(controller, methodParams.ToArray())!;
                        await task.ConfigureAwait(false);
                    }
                    else
                    {
                        invokeContext.Method.Invoke(controller, methodParams.ToArray());
                    }
                    hasFound = true;
                }
                break;
            }
        }
        if (!hasFound)
        {
            throw new KeyNotFoundException($"The routing key was not found for command '{commandText}'");
        }
    }
}