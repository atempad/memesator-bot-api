using App.Attributes;
using App.Models.API;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Services.CommandHandlers;

[BotCommandHandler]
[BotCommandRoute]
public class ProcessUrlCommandHandler(
    ProcessUrlCommand processUrlCommand) : IBotCommandHandler
{
    [BotCommandRoute]
    [RequiredPermission(Permission.AddPosts)]
    public async Task ProcessUrlAsync(InvokingContext invoker, string urlString, 
        CancellationToken cancellationToken = default)
    {
        await processUrlCommand.Setup(invoker.UserId, urlString)
            .InvokeAsync(cancellationToken);
    }
}