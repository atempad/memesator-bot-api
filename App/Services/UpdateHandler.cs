using System.Net;
using App.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private Container _subscribersContainer;
    
    public UpdateHandler(ITelegramBotClient botClient, CosmosClient cosmosClient, ILogger<UpdateHandler> logger)
    {
         _botClient = botClient;
        _subscribersContainer = cosmosClient.GetContainer("MemesatorDB", "Subscribers");
        _logger = logger;
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message }                       => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
            _                                              => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        switch (messageText.Split(' ')[0])
        {
            case "/subscribe":
                await Subscribe(_botClient, message, cancellationToken);
                break;
            case "/unsubscribe":
                await Unsubscribe(_botClient, message, cancellationToken);
                break;
            default:
                await Replay(_botClient, message, cancellationToken);
                break;
        }
    }

    private async Task Subscribe(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var subscriber = new Subscriber
        {
            Id = message.Chat.Id.ToString(),
            ChatId = message.Chat.Id,
            SubscriptionDate = DateTime.UtcNow,
            FirstName = message.Chat.FirstName,
            LastName = message.Chat.LastName,
            Username = message.Chat.Username,
            Location = message.Chat.Location,
        };
        
        string responseMessage;
        try
        {
            await _subscribersContainer.CreateItemAsync(subscriber, new PartitionKey(subscriber.Id),
                cancellationToken: cancellationToken);
            responseMessage = "Successfully subscribed";
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            responseMessage = "Already subscribed";
        }
        catch (Exception ex)
        {
            responseMessage = "Something went wrong, please try again later";
            _logger.LogInformation(ex.Message);
        }

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: responseMessage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
    
    private async Task Unsubscribe(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        string messagerId = message.Chat.Id.ToString();
        string responseMessage;
        try
        {
            await _subscribersContainer.DeleteItemAsync<Subscriber>(messagerId, new PartitionKey(messagerId), 
                cancellationToken: cancellationToken);
            responseMessage = "Successfully unsubscribed";
        }
        catch (Exception ex)
        {
            responseMessage = "Something went wrong, please try again later";
            _logger.LogInformation(ex.Message);
        }

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: responseMessage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
    
    private async Task Replay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        string messageText = message.Text!;
        if (!messageText.Contains("www.instagram"))
        {
            return;
        }
        messageText = messageText.Replace("www.", "dd");
        List<Task<Message>> tasks = [];
        string messagerId = message.Chat.Id.ToString();
        try
        {
            await _subscribersContainer.ReadItemAsync<Subscriber>(messagerId, new PartitionKey(messagerId),
                cancellationToken: cancellationToken);
            using FeedIterator<Subscriber> feedIterator = _subscribersContainer.GetItemQueryIterator<Subscriber>();
            while (feedIterator.HasMoreResults)
            {
                foreach (var subscriber in await feedIterator.ReadNextAsync(cancellationToken))
                {
                    tasks.Add(botClient.SendTextMessageAsync(
                        chatId: subscriber.ChatId,
                        text: messageText,
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken));
                }
            }
        }
        catch (Exception _)
        {
            tasks.Add(botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: messageText,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken));
        }
        await Task.WhenAll(tasks);
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient tgBotClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}