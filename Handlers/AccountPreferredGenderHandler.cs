using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;

public class AccountPreferredGenderHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountPreferredGenderHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountPreferredGenderHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.


        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        if (!CanHandle(user, update))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        if ((text == "Woman") || (text == "Man") || (text == "Both"))
        {
            using var context = _contextFactory.CreateDbContext();

            user.CurrentHandler = _nextHandler.Name;
            user.PreferGender = text;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Write semething bout you",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
        }


#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
}