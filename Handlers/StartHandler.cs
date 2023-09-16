using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;
public class StartHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StartHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "StartHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();


        long chatId = update.Message.Chat.Id;

        if (user != null)
        {
            user.CurrentHandler = _nextHandler.Name;
            user.Direction = true;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var newUser = new Models.User
            {
                ChatId = chatId,
                Language = "English",
                CurrentHandler = _nextHandler.Name,
                TurnOff = true,
            };
            await context.Users.AddAsync(newUser, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "English","Русский" },
                new KeyboardButton[] { "Українська" },
            })
            {
                ResizeKeyboard = true
            };




        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Choose language below",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }
}
