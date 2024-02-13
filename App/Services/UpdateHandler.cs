using System.Net;
using App.Models;
using Microsoft.Azure.Cosmos;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace App.Services;

public class UpdateHandler(
    ITelegramBotClient _botClient, 
    CosmosClient _cosmosClient, 
    ILogger<UpdateHandler> _logger)
{
    private readonly Container _subscribersContainer = _cosmosClient.GetContainer(
        Constants.DB.Id, 
        Constants.DB.Containers.Subscribers);

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
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