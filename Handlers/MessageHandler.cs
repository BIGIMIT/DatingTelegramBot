using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;

public abstract class MessageHandler
{
    public abstract string? Name { get; }
    protected MessageHandler? _nextHandler;
    protected MessageHandler? _previousHandler;

    public void SetNextHandler(MessageHandler messageHandler)
    {
        _nextHandler = messageHandler;
        messageHandler._previousHandler = this;
    }
    protected bool CanHandle(Models.User? user, Update update)
    {
        if (update?.Message?.Text == "/start")
        {
            return true;
        }
        else if (user != null && user.CurrentHandler == Name)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update))
        {
            if (user?.Direction == true && _nextHandler != null)
            {
                await _nextHandler.HandleAsync(user, botClient, update, cancellationToken);
            }
            // Если Direction == false и предыдущий обработчик существует, передаем обработку предыдущему
            else if (user?.Direction == false && _previousHandler != null)
            {
                await _previousHandler.HandleAsync(user, botClient, update, cancellationToken);
            }
            else
            {
                if (update.Message == null) return;
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Sorry, I couldn't process your request. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }

}
