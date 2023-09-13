using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountPreferredGenderHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountPreferredGenderHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountPreferredGenderHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        using var context = _contextFactory.CreateDbContext();

        if (text == PhraseDictionary.GetPhrase(user.Language, Phrases.Woman))
        {
            user.CurrentHandler = _nextHandler.Name;
            user.PreferGender = "Woman";


        }
        else if (text == PhraseDictionary.GetPhrase(user.Language, Phrases.Man))
        {

            user.CurrentHandler = _nextHandler.Name;
            user.PreferGender = "Man";
        }
        else if (text == PhraseDictionary.GetPhrase(user.Language, Phrases.Both))
        {

            user.CurrentHandler = _nextHandler.Name;
            user.PreferGender = "Both";
        }
        else
        {
            return;
        }

        await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: PhraseDictionary.GetPhrase(user.Language, Phrases.Write_semething_bout_you),
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}