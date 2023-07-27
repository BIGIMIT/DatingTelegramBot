using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;

public class AccountPhotoHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountPhotoHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountPhotoHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.


        if (!CanHandle(user, update))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }


        var fileId = update.Message.Photo.Last().FileId;
        long chatId = update.Message.Chat.Id;

        string destinationDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UsersPhotos");
        Directory.CreateDirectory(destinationDirectory);
        string destinationFilePath = Path.Combine(destinationDirectory, fileId + ".jpg");
        await using Stream fileStream = System.IO.File.Create(destinationFilePath);

        var file = await botClient.GetInfoAndDownloadFileAsync(
            fileId: fileId,
            destination: fileStream,
            cancellationToken: cancellationToken);

        using var context = _contextFactory.CreateDbContext();

        user.CurrentHandler = _nextHandler.Name;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);


        //if (user.Photos.Count == 3)
        //{
        //    await botClient.SendTextMessageAsync(
        //            chatId: chatId,
        //            text: "These are 3 out of 3 photos, do you want to change them? Or finish registration",
        //            replyMarkup: new ReplyKeyboardRemove(),
        //            cancellationToken: cancellationToken);
        //    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        //    {
        //        new KeyboardButton[] { "Change" },
        //        new KeyboardButton[] { "Finish registration" }
        //    })
        //    {
        //        ResizeKeyboard = true
        //    };
        //}
        //else
        //{

        //    await botClient.SendTextMessageAsync(
        //            chatId: chatId,
        //            text: "Add 1 more photo",
        //            replyMarkup: new ReplyKeyboardRemove(),
        //            cancellationToken: cancellationToken);
        //    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        //    {
        //    new KeyboardButton[] { "Yes" },
        //    new KeyboardButton[] { "No" },
        //})
        //    {
        //        ResizeKeyboard = true
        //    };
        //}

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}