using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountGenderHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountGenderHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountGenderHandler";

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
            user.Gender = "Woman";
        }
        else if (text == PhraseDictionary.GetPhrase(user.Language, Phrases.Man))
        {
            user.CurrentHandler = _nextHandler.Name;
            user.Gender = "Man";
        }
        else
        {
            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: PhraseDictionary.GetPhrase(user.Language, Phrases.Use_the_keyboard_below),
                   cancellationToken: cancellationToken);
            return;
        }

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
                    new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Woman) },
                    new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Man) },
                    new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Both) },
                })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: PhraseDictionary.GetPhrase(user.Language, Phrases.Choose_preferred_gender),
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
    }
}