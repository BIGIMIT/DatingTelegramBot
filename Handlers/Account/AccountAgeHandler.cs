using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountAgeHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountAgeHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountAgeHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) ||
            user == null ||
            update.Message == null ||
            update.Message.Text == null ||
            _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        using var context = _contextFactory.CreateDbContext();
        if (int.TryParse(text, out int number) && number < 100 && number >= 18)
        {
            user.Age = number;
        }
        else
        {
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Age must be number that less than 100",
            cancellationToken: cancellationToken);
            return;
        }

        
        user.CurrentHandler = _nextHandler.Name;

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Woman" },
            new KeyboardButton[] { "Man" },
        })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose your gender",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);


    }
}