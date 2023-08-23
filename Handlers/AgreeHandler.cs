using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;

public class AgreeHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AgreeHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AgreeHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;

        if (update.Message.Text == "I agree")
        {
            user.CurrentHandler = _nextHandler.Name;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
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


    }
}