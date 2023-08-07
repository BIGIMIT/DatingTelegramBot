using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers.Account;

public class AccountNameHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountNameHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountNameHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;

        user.CurrentHandler = _nextHandler.Name;
        user.Name = update.Message.Text;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Now enter your age",
            cancellationToken: cancellationToken);
    }
}