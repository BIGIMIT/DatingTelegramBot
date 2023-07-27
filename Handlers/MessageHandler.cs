using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;

public abstract class MessageHandler
{
    public abstract string? Name { get; }
    protected MessageHandler? _nextHandler;

    public void SetNextHandler(MessageHandler messageHandler)
    {
        _nextHandler = messageHandler;
    }
    protected bool CanHandle(Models.User? user, Update update)
    {
        if (update?.Message?.Text == "/start")
        {
            return true;
        }
        else if (user != null && user.CurrentHandler == Name && Name == "AccountPhotoHandler" && update?.Message?.Photo != null)
        {
            return true;
        }
        else if (user != null && user.CurrentHandler == Name && update?.Message?.Text != null)
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
        if (!CanHandle(user, update) && _nextHandler != null)
        {
            // Если этот обработчик не может обработать запрос, передать его следующему обработчику
            await _nextHandler.HandleAsync(user, botClient, update, cancellationToken);
        }
    }

}
