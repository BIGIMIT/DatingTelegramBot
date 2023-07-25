using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;
public class StartHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StartHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "StartHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using (var context = _contextFactory.CreateDbContext())
        {

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            long chatId = update.Message.Chat.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (_nextHandler == null)
            {
                _nextHandler = this;
            }
            // Создание пользователя
            var newUser = new Models.User
            {
                ChatId = chatId,
                CurrentHandler = _nextHandler.Name,
                TurnOff = true,
            };
            if (user != null)
            {
                user.CurrentHandler = _nextHandler.Name;
                await UpdateAsync(user, context);
            }
            else
            {
                await context.Users.AddAsync(newUser, cancellationToken);
                await context.SaveChangesAsync();
            }
            
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Welcome",
            cancellationToken: cancellationToken);
        }

    }
}
