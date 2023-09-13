using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Searching;
 
public class SearchingProfileHandle : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SearchingProfileHandle(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SearchingProfileHandle";
    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        if (!CanHandle(user, update) || user == null || update.Message == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        using var context = _contextFactory.CreateDbContext();

        if (update.Message.Text == PhraseDictionary.GetPhrase(user.Language, Phrases.Change_profile)) 
        {
            user.CurrentHandler = "ChangeAccountHandler";
            user.Direction = true;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        else if (update.Message.Text == PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching))
        {

            user.CurrentHandler = "SearchingStartHandler";
            user.Direction = false;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;

        var text = BuildUserDescription(user);

        user.Photos ??= new();
        var photo = user.Photos.FirstOrDefault();
        
        try
        {
            if (photo == null || photo.Path == null) return;
            var stream = new FileStream(photo.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                    new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching) },
                    new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Change_profile) },
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
        catch
        {
            await botClient.SendTextMessageAsync(
                chatId,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Try_again),
                cancellationToken: cancellationToken);
        }


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
}
