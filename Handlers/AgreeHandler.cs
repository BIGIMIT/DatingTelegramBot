using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;

public class AgreeHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AgreeHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AgreeHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();
        

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            long chatId = update.Message.Chat.Id;

            user.CurrentHandler = _nextHandler.Name;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "I agree" },
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Accept the agreement",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        

    }
}