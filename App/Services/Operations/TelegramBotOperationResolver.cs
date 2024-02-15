using System.Diagnostics;
using App.Models.API;
using App.Models.DB;
using App.Repositories;
using App.Services.Telegram;

namespace App.Services.Operations;

public class TelegramBotOperationResolver(
    ITelegramBotApi _telegramBotApi,
    IUserRepository _userRepository,
    ISubscriptionRepository _subscriptionRepository) : ITelegramBotOperationResolver
{
    private TelegramUpdate? _update;
    
    public void Setup(TelegramUpdate update)
    {
        _update = update;
    }

    public IAsyncOperation? Resolve()
    {
        Debug.Assert(_update is not null, "Ensure that Setup is called with a valid agruments before calling Resolve.");
        IAsyncOperation? command = default;
        if (_update.Message is { Text: not null })
        {
            TelegramUser sender = _update.Message.From!;
            TelegramChat chat = _update.Message.Chat;
            var invoker = new BotUser
            {
                Id = sender.Username!,
                Username = sender.Username,
                ChatId = chat.Id.ToString()
            };
            string[] commandWithArgumens = _update.Message.Text.Split(' ');
            string commandName = commandWithArgumens.FirstOrDefault(string.Empty);
            var argumentIdx = 1;
            switch (commandName)
            {
                case "/start":
                {
                    command = new StartOperation(invoker, _telegramBotApi, _userRepository);
                    break;
                }
                case "/stop":
                {
                    command = new StopOperation(invoker, _telegramBotApi, _userRepository);
                    break;
                }
                case "/subscribechat":
                {
                    var subscriberId = commandWithArgumens.ElementAtOrDefault(argumentIdx++);
                    var broadcasterId = commandWithArgumens.ElementAtOrDefault(argumentIdx);
                    if (!string.IsNullOrWhiteSpace(subscriberId) 
                        && !string.IsNullOrWhiteSpace(broadcasterId))
                    {
                        command = new SubscribeChatOperation(invoker, subscriberId, broadcasterId, _telegramBotApi,
                            _userRepository, _subscriptionRepository);
                    }
                    break;
                }
                case "/subscribeuser":
                {
                    var subscriberId = commandWithArgumens.ElementAtOrDefault(argumentIdx++);
                    var broadcasterId = commandWithArgumens.ElementAtOrDefault(argumentIdx);
                    if (!string.IsNullOrWhiteSpace(subscriberId) 
                        && !string.IsNullOrWhiteSpace(broadcasterId))
                    {
                        command = new SubscribeUserOperation(invoker, subscriberId, broadcasterId, _telegramBotApi,
                            _userRepository, _subscriptionRepository);
                    }
                    break;
                }
                case "/subscribe":
                {
                    var subscriberId = invoker.Id;
                    var broadcasterId = commandWithArgumens.ElementAtOrDefault(argumentIdx);
                    if (!string.IsNullOrWhiteSpace(subscriberId) 
                        && !string.IsNullOrWhiteSpace(broadcasterId))
                    {
                        command = new SubscribeUserOperation(invoker, subscriberId, broadcasterId, _telegramBotApi,
                            _userRepository, _subscriptionRepository);
                    }
                    break;
                }
                default:
                {
                    if (Uri.IsWellFormedUriString(commandName, UriKind.Absolute))
                    {
                        command = new BroadcastMediaOperation(invoker, commandName, _telegramBotApi, 
                            _subscriptionRepository, _userRepository);
                    }
                    break;
                }
            }
        }
        return command;
    }
}