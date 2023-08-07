using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountGenderHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountGenderHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountGenderHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        if (text == "Woman" || text == "Man")
        {
            using var context = _contextFactory.CreateDbContext();

            user.CurrentHandler = _nextHandler.Name;
            user.Gender = text;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                    new KeyboardButton[] { "Woman" },
                    new KeyboardButton[] { "Man" },
                    new KeyboardButton[] { "Both" },
                })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose preferred gender",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Use the keyboard below",
                   cancellationToken: cancellationToken);
        }
    }
}