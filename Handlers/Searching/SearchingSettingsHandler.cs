using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingSettingsHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SearchingSettingsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SearchingSettingsHandler";

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
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var newUser = new Models.User
            {
                ChatId = chatId,
                CurrentHandler = _nextHandler.Name,
                TurnOff = true,
            };
            await context.Users.AddAsync(newUser, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
                new KeyboardButton[] { "Continue" },
            })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Welcome",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }
}