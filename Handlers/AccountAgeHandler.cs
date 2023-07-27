using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;

public class AccountAgeHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountAgeHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountAgeHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if ((!CanHandle(user, update)))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        using var context = _contextFactory.CreateDbContext();
        if (int.TryParse(text, out int number) && number < 100)
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.


    }
}