using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers.Account;

public class AccountViewOrComplete : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountViewOrComplete(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountViewOrComplete";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null ||
            (update.Message.Text != "Complete registration" && update.Message.Text != "View profile"))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();
        long chatId = update.Message.Chat.Id;

        if (update.Message.Text == "View profile")
        {
            user.CurrentHandler = "SendUserProfileHandler";
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
            context.Dispose();

            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        else if (update.Message.Text == "Complete registration")
        {

            user.CurrentHandler = "SearchingStartHandler";
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
            context.Dispose();

            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

    }
}