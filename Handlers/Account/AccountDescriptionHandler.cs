using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountDescriptionHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountDescriptionHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountDescriptionHandler";

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

        user.CurrentHandler = _nextHandler.Name;
        user.Description = text;

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: PhraseDictionary.GetPhrase(user.Language, Phrases.Thats_almost_all_it_remains_only_to_add_a_photo),
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);

    }
}