using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Update = Telegram.Bot.Types.Update;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingMatches : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SearchingMatches(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SearchingMatches";
    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();
        long chatId = update.Message.Chat.Id;

        string next = PhraseDictionary.GetPhrase(user.Language, Phrases.Next);
        string matches = PhraseDictionary.GetPhrase(user.Language, Phrases.Matches);
        string back = PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching);

        if (user == null) return;

        if (update.Message.Text == "⚙️")
        {
            await HandleSettings(user, update, botClient, cancellationToken);
        }
        else if (update.Message.Text == next || update.Message.Text == matches)
        {
            await HandleNextProfile(user, update, botClient, cancellationToken);
        }
        else if (update.Message.Text == back)
        {
            await HandleBackToSearching(botClient, update, user, cancellationToken);
        }

    }

    private async Task HandleBackToSearching(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        using var context = _contextFactory.CreateDbContext();
        user.CurrentHandler = "SearchingStartHandler";
        user.Direction = false;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await base.HandleAsync(user, botClient, update, cancellationToken);
        return;
    }

    private async Task HandleSettings(Models.User user, Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        if (user == null) return;
        using var context = _contextFactory.CreateDbContext();
        user.CurrentHandler = "SearchingSettingsHandler";
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.View_my_profile) },
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Matches) },
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Stop_searching) },
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching) },
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            user.ChatId,
            PhraseDictionary.GetPhrase(user.Language, Phrases.Select_an_item),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
        return;
    }

    private async Task HandleNextProfile(Models.User user, Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {

        using var context = _contextFactory.CreateDbContext();

        var nextUsers = await context.Users
        .Include(u => u.Photos)
        .Where(u => context.UserViews.Any(uv1 =>
            uv1.ViewerId == user.Id && uv1.ViewedId == u.Id && uv1.Like == true && uv1.WasShown == false) &&
            context.UserViews.Any(uv2 =>
            uv2.ViewerId == u.Id && uv2.ViewedId == user.Id && uv2.Like == true))
        .ToListAsync();

        if (nextUsers.FirstOrDefault() is not { } nextUser)
        {
            await ProfileNotFoundAsync(botClient, update, user, cancellationToken);
            return;
        }



        var text = await BuildUserDescription(nextUser, user);
        nextUser.Photos ??= new List<Photo>();
        var photo = nextUser.Photos.FirstOrDefault();

        if (photo != null && photo.Path != null)
        {
            using var stream = new FileStream(photo.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Next), "⚙️" },
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching)},
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendPhotoAsync(
                chatId: user.ChatId,
                photo: InputFile.FromStream(stream: stream, fileName: photo.FileId?.ToString()),
                caption: text,
                parseMode: ParseMode.Html,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                user.ChatId,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Try_again),
                cancellationToken: cancellationToken);
            return;
        }


        var currentUserViews = await context.UserViews
        .Where(uv => uv.ViewerId == user.Id && uv.ViewedId == nextUser.Id).ToListAsync();

        var currentUserView = currentUserViews.FirstOrDefault();
        if (currentUserView != null)
        {
            currentUserView.WasShown = true;
        }
        else
        {
            currentUserView = new() { 
                WasShown = true,
                ViewedId = user.Id,
                ViewerId = nextUser.Id,
                Like = true,
            };
        }
        context.UserViews.Update(currentUserView);
        await context.SaveChangesAsync(cancellationToken);

    }


    public async Task ProfileNotFoundAsync(ITelegramBotClient botClient, Update update, Models.User user, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Next), "⚙️" },
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching)},
            })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(
            user.ChatId,
            PhraseDictionary.GetPhrase(user.Language, Phrases.No_more_matches_available),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);

    }


    private async Task<string> BuildUserDescription(Models.User nextUser, Models.User user)
    {
        StringBuilder sb = new();


        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Name)}: ");
        sb.AppendLine(nextUser.Name);

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Age)}: ");
        sb.AppendLine(nextUser.Age.ToString());

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Gender)}: ");
        sb.AppendLine(nextUser.Gender);

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Description)}: ");
        sb.AppendLine(nextUser.Description);

        UserView? userView = await GetUserView(nextUser, user);

        if (userView != null && !userView.Message.IsNullOrEmpty())
        {
            sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Messege)}: ");
            sb.AppendLine(userView.Message);
        }

        sb.Append("\n");
        sb.AppendLine("@"+nextUser.UserName);

        return sb.ToString();
    }
    public async Task<UserView?> GetUserView(Models.User nextUser, Models.User user)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var userView = await context.UserViews
            .Where(uv => uv.ViewerId == nextUser.Id && uv.ViewedId == user.Id)
            .FirstOrDefaultAsync();

        return userView;
    }
}