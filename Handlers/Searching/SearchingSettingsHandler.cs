using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Update = Telegram.Bot.Types.Update;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingSettingsHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SearchingSettingsHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SearchingSettingsHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }


        using var context = _contextFactory.CreateDbContext();

        var chatId = update.Message.Chat.Id;

        string viewMyProfile = PhraseDictionary.GetPhrase(user.Language, Phrases.View_my_profile);
        string matches = PhraseDictionary.GetPhrase(user.Language, Phrases.Matches);
        string stopSearching = PhraseDictionary.GetPhrase(user.Language, Phrases.Stop_searching);
        string backToSearching = PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching);

        if (update.Message.Text == viewMyProfile)
        {
            await HandleViewMyProfile(botClient, update, user, cancellationToken);
        }
        else if (update.Message.Text == matches)
        {
            await HandleMatches(botClient, update, chatId, user, cancellationToken);
        }
        else if (update.Message.Text == stopSearching)
        {
            await HandleStopSearching(botClient, chatId, user, cancellationToken);
        }
        else if (update.Message.Text == backToSearching)
        {
            await HandleBackToSearching(botClient, update, user, cancellationToken);
        }

        user.Direction = true;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        return;
    }
    private async Task HandleViewMyProfile(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {

        if (user == null || _nextHandler == null) return;
        using var context = _contextFactory.CreateDbContext();
        
            user.CurrentHandler = "SearchingProfileHandle";
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            await base.HandleAsync(user, botClient, update, cancellationToken);
        return;
    }

    private async Task HandleMatches(ITelegramBotClient botClient, Update update, long chatId, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        using var context = _contextFactory.CreateDbContext();

        user.CurrentHandler = "SearchingMatches"; 
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await base.HandleAsync(user, botClient, update, cancellationToken);
        return;
    }

    private async Task HandleStopSearching(ITelegramBotClient botClient, long chatId, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        using var context = _contextFactory.CreateDbContext();

        if (user == null) return;

        user.CurrentHandler = "ResumeSearchingHandler";
        user.TurnOff = true;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Resume_searching) },
            })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: PhraseDictionary.GetPhrase(user.Language, Phrases.Your_account_has_become_invisible_to_other_users),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private async Task HandleBackToSearching(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        user.CurrentHandler = "SearchingStartHandler";
        user.Direction = false;
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await base.HandleAsync(user, botClient, update, cancellationToken);
    }

}