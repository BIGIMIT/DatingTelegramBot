using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using DatingTelegramBot.Services;

namespace DatingTelegramBot.Handlers.Account;

public class AccountUsernameHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountUsernameHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountUsernameHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;
        string? username = update.Message?.From?.Username;

        if (string.IsNullOrEmpty(username))
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: PhraseDictionary.GetPhrase(user.Language, Phrases.Set_up_your_Username_and_try_again),
                cancellationToken: cancellationToken);
            return;
        }

        user.CurrentHandler = _nextHandler.Name;
        user.UserName = username;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await base.HandleAsync(user, botClient, update, cancellationToken);
        return;


    }
}