using DatingTelegramBot.Models;
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
        if (!CanHandle(user, update) || update.Message == null || update.Message.Text == null )
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        var chatId = update.Message.Chat.Id;

        switch (update.Message.Text)
        {
            case "View my profile":
                await HandleViewMyProfile(botClient, update, user, cancellationToken);
                break;

            case "Matches":
                await HandleMatches(botClient, chatId, user, cancellationToken);
                break;

            case "Stop searching":
                await HandleStopSearching(botClient, chatId, user, cancellationToken);
                break;

            case "Back to searching":
                await HandleBackToSearching(botClient, update, user, cancellationToken);
                break;

        }
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

    private async Task HandleMatches(ITelegramBotClient botClient, long chatId, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        using var context = _contextFactory.CreateDbContext();
        var matchedUsers = await context.UserViews
            .Include(uv => uv.Viewer)
            .Where(uv => uv.ViewedId == user.Id && uv.Like == true)
            .Select(uv => uv.Viewer)
            .ToListAsync(cancellationToken);

        if (matchedUsers.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "You have no matches yet.",
                cancellationToken: cancellationToken);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("Your matches:");

        foreach (var matchedUser in matchedUsers)
        {
            sb.AppendLine($"Name: {matchedUser.Name}, Age: {matchedUser.Age}, Description: {matchedUser.Description}");
        }

        await botClient.SendTextMessageAsync(
            chatId,
            sb.ToString(),
            cancellationToken: cancellationToken);
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
                new KeyboardButton[] { "Resume searching" },
            })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Your account has become invisible to other users. To go back to the search click on the button.",
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