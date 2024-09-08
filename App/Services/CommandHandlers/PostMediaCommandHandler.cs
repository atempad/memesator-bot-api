using App.Attributes;
using App.Models.API;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Services.CommandHandlers;

[BotCommandHandler]
[BotCommandRoute]
public class PostMediaCommandHandler(
    PostMediaCommand postMediaCommand) : IBotCommandHandler
{
    [BotCommandRoute]
    [RequiredPermission(Permission.AddPosts)]
    public async Task PostMediaAsync(InvokingContext invoker, string urlString, 
        CancellationToken cancellationToken = default)
    {
        await postMediaCommand.Setup(invoker.ChatId, urlString)
            .InvokeAsync(cancellationToken);
    }
}