using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingStartHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private const string CompleteRegistration = "Complete registration";
    private const string Like = "❤️";
    private const string Dislike = "👎";
    private const string Message = "✉️";
    private const string Settings = "⚙️";

    public SearchingStartHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public override string? Name => "SearchingStartHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!IsValidUpdate(user, update) || update.Message == null || update.Message.Text == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        var chatId = update.Message.Chat.Id;

        using var context = _contextFactory.CreateDbContext();

        if (string.IsNullOrEmpty(user?.PreferGender)) return;

        var nextUser = await GetNextProfileForUser(user.Id, user.PreferGender);
        var currentUser = await GetCurrentProfileForUser(user.Id);

        switch (update.Message.Text)
        {
            case CompleteRegistration:
                await HandleCompleteRegistration(botClient, chatId, user, nextUser, cancellationToken);
                break;

            case Like:
                await HandleLikeOrDislike(botClient, chatId, user, true, nextUser, cancellationToken);
                break;

            case Dislike:
                await HandleLikeOrDislike(botClient, chatId, user, false, nextUser, cancellationToken);
                break;

            case Message:
                await HandleMessage(botClient, chatId, user, context, cancellationToken);
                break;

            case Settings:
                await HandleSettings(botClient, chatId, user, context, cancellationToken);
                break;
        }
    }
    private bool IsValidUpdate(Models.User? user, Update update)
    {
        return CanHandle(user, update) && user != null && update.Message?.Text != null;
    }

    private async Task HandleCompleteRegistration(ITelegramBotClient botClient, long chatId, Models.User user, Models.User? nextUser, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();

        if (nextUser == null)
        {
            await ProfileNotFoundAsync(botClient, chatId, cancellationToken);
            return;
        }

        var userView = new UserView
        {
            ViewerId = user.Id,
            ViewedId = nextUser.Id,
            Like = null
        };

        context.UserViews.Add(userView);
        await context.SaveChangesAsync(cancellationToken);
        await SendProfilePhotoAsync(nextUser, botClient, chatId, cancellationToken);
    }

    private async Task HandleLikeOrDislike(ITelegramBotClient botClient, long chatId, Models.User user, bool isLike, Models.User? nextUser, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();

        var currentUserView = await context.UserViews
            .Where(uv => uv.ViewerId == user.Id)
            .OrderByDescending(uv => uv.ViewedId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserView != null)
        {
            currentUserView.Like = isLike;
            context.UserViews.Update(currentUserView);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            Console.WriteLine("HandleLikeOrDislike - currentUserView == null ");
        }

        if (nextUser == null)
        {
            await ProfileNotFoundAsync(botClient, chatId, cancellationToken);
            return;
        }

        await SendProfilePhotoAsync(nextUser, botClient, chatId, cancellationToken);
    }

    private async Task HandleMessage(ITelegramBotClient botClient, long chatId, Models.User user, ApplicationDbContext context, CancellationToken cancellationToken)
    {
        user.CurrentHandler = "SearchingSettingsHandler";
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await botClient.SendTextMessageAsync(
            chatId,
            "Write some message",
            cancellationToken: cancellationToken);
    }

    private async Task HandleSettings(ITelegramBotClient botClient, long chatId, Models.User user, ApplicationDbContext context, CancellationToken cancellationToken)
    {
        user.CurrentHandler = "SearchingMessageHandler";
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
        new KeyboardButton[] { "Continue 1" },
        new KeyboardButton[] { "Continue 2" },
        new KeyboardButton[] { "Continue 3" },
        new KeyboardButton[] { "Continue 4" },
    })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId,
            "Select an item",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private static string BuildUserDescription(Models.User user)
    {
        StringBuilder sb = new();

        sb.Append("Name: ");
        sb.AppendLine(user.Name);

        sb.Append("Age: ");
        sb.AppendLine(user.Age.ToString());

        sb.Append("Description: ");
        sb.AppendLine(user.Description);

        return sb.ToString();
    }
    public async Task<Models.User?> GetNextProfileForUser(int userId, string preferGender)
    {
        using var context = _contextFactory.CreateDbContext();

        // Используем прямой запрос для проверки, видел ли пользователь конкретного человека.
        var nextUser = await context.Users
            .Include(u => u.Photos)
            .Where(u => !context.UserViews.Any(uv => uv.ViewerId == userId && uv.ViewedId == u.Id)
                        && u.Id != userId
                        && (preferGender == "Both" || u.Gender == preferGender))
            .FirstOrDefaultAsync();

        return nextUser;
    }

    public async Task<Models.User?> GetCurrentProfileForUser(int userId)
    {
        using var context = _contextFactory.CreateDbContext();

        // Используем SplitQuery для разделения запросов и уменьшения времени выполнения.
        Models.User? currentUser = await context.Users.AsSplitQuery()
            .Include(u => u.Photos)
            .Include(u => u.ViewedUsers)
            .Where(u => !context.UserViews.Any(uv => uv.ViewerId == userId && uv.ViewedId == u.Id)
                        && u.Id != userId)
            .OrderByDescending(u => u.Id)
            .FirstOrDefaultAsync();

        return currentUser;
    }

    public static async Task ProfileNotFoundAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            chatId,
            "No more profiles available",
            cancellationToken: cancellationToken);
    }
    public static async Task SendProfilePhotoAsync(Models.User nextUser, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
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
                "Could not find file",
                cancellationToken: cancellationToken);
        }
    }


}