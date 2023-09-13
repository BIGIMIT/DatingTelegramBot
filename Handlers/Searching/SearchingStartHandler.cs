using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Update = Telegram.Bot.Types.Update;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingStartHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;
    
    private const string Like = "❤️";
    private const string Dislike = "👎";
    private const string Message = "✉️";
    private const string Settings = "⚙️";

    public SearchingStartHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public override string? Name => "SearchingStartHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!IsValidUpdate(user, update) || user == null || update.Message == null || update.Message.Text == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        var chatId = update.Message.Chat.Id;

        string CompleteRegistration = PhraseDictionary.GetPhrase(user.Language, Phrases.Complete_registration);
        string BackToSearching = PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching);
        string ResumeSearching = PhraseDictionary.GetPhrase(user.Language, Phrases.Resume_searching);

        using var context = _contextFactory.CreateDbContext();

        user.Direction = true;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        
        if (string.IsNullOrEmpty(user?.PreferGender)) return;

        Models.User? nextUser = await GetNextProfileForUser(user, user.PreferGender);
        if (nextUser == null) 
        { 
            await ProfileNotFoundAsync(botClient, chatId, user, cancellationToken); return; 
        }
        else if (update.Message.Text == CompleteRegistration ||
            update.Message.Text == BackToSearching ||
            update.Message.Text == ResumeSearching)
        {
            await HandleCompleteRegistration(botClient, chatId, user, nextUser, cancellationToken);
        }
        else if (update.Message.Text == Like || update.Message.Text == Dislike)
        {
            bool isLike = update.Message.Text == Like;
            await HandleLikeOrDislike(botClient, chatId, user, isLike, nextUser, cancellationToken);
        }
        else if (update.Message.Text == Message)
        {
            await HandleMessage(botClient, chatId, user, context, cancellationToken);
        }
        else if (update.Message.Text == Settings)
        {
            await HandleSettings(botClient, chatId, user, cancellationToken);
        }

    }
    private bool IsValidUpdate(Models.User? user, Update update)
    {
        return CanHandle(user, update) && user != null && update.Message?.Text != null;
    }

    private async Task HandleCompleteRegistration(ITelegramBotClient botClient, long chatId, Models.User user, Models.User? nextUser, CancellationToken cancellationToken)
    {
        if (nextUser == null)
        {
            await ProfileNotFoundAsync(botClient, chatId, user, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();



        user.ViewedUsers ??= new List<UserView>();

        if (nextUser != null)
        {
            var currentUserView = await context.UserViews
                .FirstOrDefaultAsync(uv => uv.ViewerId == user.Id && uv.ViewedId == nextUser.Id, cancellationToken);

            if (currentUserView == null)
            {
                currentUserView = new UserView
                {
                    ViewerId = user.Id,
                    ViewedId = nextUser.Id,
                    Like = null
                };
                context.UserViews.Add(currentUserView);
            }
            else
            {
                currentUserView.Like = null;
                context.UserViews.Update(currentUserView);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        if (nextUser == null)
        {
            await ProfileNotFoundAsync(botClient, chatId, user, cancellationToken);
            return;
        }
        else
            await SendProfilePhotoAsync(user, nextUser, botClient, chatId, cancellationToken);
    }

    private async Task HandleLikeOrDislike(ITelegramBotClient botClient, long chatId, Models.User user, bool isLike, Models.User nextUser, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();
        if (nextUser != null)
        {
            var currentUserView = await context.UserViews
                .FirstOrDefaultAsync(uv => uv.ViewerId == user.Id && uv.ViewedId == nextUser.Id, cancellationToken);

            if (currentUserView == null)
            {
                currentUserView = new UserView
                {
                    ViewerId = user.Id,
                    ViewedId = nextUser.Id,
                    Like = isLike
                };
                context.UserViews.Add(currentUserView);
            }
            else
            {
                currentUserView.Like = isLike;
                context.UserViews.Update(currentUserView);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

#pragma warning disable CS8604 // Possible null reference argument.
        var updatedNextUser = await GetNextProfileForUser(user, user.PreferGender);
#pragma warning restore CS8604 // Possible null reference argument.

        if (updatedNextUser == null)
        {
            await ProfileNotFoundAsync(botClient, chatId, user, cancellationToken);
            return;
        }

        await SendProfilePhotoAsync(user, updatedNextUser, botClient, chatId, cancellationToken);
    }

    private static async Task HandleMessage(ITelegramBotClient botClient, long chatId, Models.User user, ApplicationDbContext context, CancellationToken cancellationToken)
    {
        user.CurrentHandler = "SearchingMessageHandler";
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await botClient.SendTextMessageAsync(
            chatId,
            PhraseDictionary.GetPhrase(user.Language, Phrases.Write_some_message),
            cancellationToken: cancellationToken);
    }
    private async Task HandleSettings(ITelegramBotClient botClient, long chatId, Models.User user, CancellationToken cancellationToken)
    {
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
            chatId,
            PhraseDictionary.GetPhrase(user.Language, Phrases.Select_an_item),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }
    private static string BuildUserDescription(Models.User user)
    {
        StringBuilder sb = new();

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Name)}: ");
        sb.AppendLine(user.Name);

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Age)}: ");
        sb.AppendLine(user.Age.ToString());

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Gender)}: ");
        sb.AppendLine(user.Gender);

        sb.Append($"{PhraseDictionary.GetPhrase(user.Language, Phrases.Description)}: ");
        sb.AppendLine(user.Description);

        return sb.ToString();
    }
    public async Task<Models.User?> GetNextProfileForUser(Models.User user, string preferGender)
    {
        using var context = _contextFactory.CreateDbContext();

        // Выбираем пользователей, которых пользователь еще не видел
        var unseenUsers = await context.Users
            .Include(u => u.Photos)
            .Where(u => (!context.UserViews.Any(uv => uv.ViewerId == user.Id && uv.ViewedId == u.Id) ||
            (context.UserViews.Any(uv => uv.ViewerId == user.Id && uv.ViewedId == u.Id && uv.Like == null)))
                        && u.Id != user.Id
                        && (preferGender == "Both" || u.Gender == preferGender))
            .ToListAsync();

        // Выбираем следующего пользователя из невиденных пользователей
        var nextUser = unseenUsers.FirstOrDefault();

        return nextUser;
    }
    public async Task ProfileNotFoundAsync(ITelegramBotClient botClient, long chatId, Models.User user, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();
        user.CurrentHandler = "SearchingSettingsHandler";
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId,
            PhraseDictionary.GetPhrase(user.Language, Phrases.No_more_profiles_available),
            cancellationToken: cancellationToken);
        await HandleSettings(botClient, chatId, user, cancellationToken);
    }
    public static async Task SendProfilePhotoAsync(Models.User user, Models.User nextUser, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var text = BuildUserDescription(nextUser);
        nextUser.Photos ??= new List<Photo>();
        var photo = nextUser.Photos.FirstOrDefault();

        if (photo != null && photo.Path != null)
        {
            using var stream = new FileStream(photo.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "❤️", "✉️", "👎", "⚙️" },
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendPhotoAsync(
                chatId: chatId,
                photo: InputFile.FromStream(stream: stream, fileName: photo.FileId?.ToString()),
                caption: text,
                parseMode: ParseMode.Html,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Try_again),
                cancellationToken: cancellationToken);
        }
    }


}