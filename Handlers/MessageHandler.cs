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
        if (!CanHandle(user, update) && _nextHandler != null)
        {
            // Если этот обработчик не может обработать запрос, передать его следующему обработчику
            await _nextHandler.HandleAsync(user, botClient, update, cancellationToken);
        }
    }
    public async Task UpdateAsync(Models.User user, ApplicationDbContext context)
    {
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            // Обработка исключения, связанного с базой данных
        }
    }

}
