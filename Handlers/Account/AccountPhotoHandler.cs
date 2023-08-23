using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountPhotoHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountPhotoHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountPhotoHandler";
    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Photo == null || _nextHandler == null || update.Message.Text != null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        string fileId = update.Message.Photo.Last().FileId;
        long chatId = update.Message.Chat.Id;
        string destinationDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UsersPhotos");
        Directory.CreateDirectory(destinationDirectory);
        string destinationFilePath = Path.Combine(destinationDirectory, fileId + ".jpg");

        await using (Stream fileStream = System.IO.File.Create(destinationFilePath))
        {
            var file = await botClient.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream,
                cancellationToken: cancellationToken);
        }

        using var context = _contextFactory.CreateDbContext();

        if (user.Photos != null)
        {
            context.Photos.RemoveRange(user.Photos);
            await context.SaveChangesAsync(cancellationToken);
        }

        user.Photos = new List<Photo>();
            user.CurrentHandler = _nextHandler.Name;
            user.Photos.Add(new Photo { FileId = fileId, Path = destinationFilePath });

            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);


            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                    new KeyboardButton[] { "View profile" },
                    new KeyboardButton[] { "Complete registration" },
                })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose next step",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);

            //else
            //{

            //    user.Photos.Add(new Photo { FileId = fileId, Path = destinationFilePath });

            //    context.Users.Update(user);
            //    await context.SaveChangesAsync(cancellationToken);

            //    await botClient.SendTextMessageAsync(
            //            chatId: chatId,
            //            text: "Please add another photo",
            //            replyMarkup: new ReplyKeyboardRemove(),
            //            cancellationToken: cancellationToken);
            //}
        

    }

}
