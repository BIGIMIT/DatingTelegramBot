﻿using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class SendUserProfileHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SendUserProfileHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SendUserProfileHandler";
    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        if (!CanHandle(user, update) || user == null || update.Message == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;

        using var context = _contextFactory.CreateDbContext();

        user.CurrentHandler = _nextHandler.Name;
        var text = BuildUserDescription(user);

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        var mediaGroup = new List<IAlbumInputMedia>();

        user.Photos ??= new();
        var photo = user.Photos.FirstOrDefault();
        try
        {
            if (photo == null || photo.Path == null) return;
            var stream = new FileStream(photo.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                    new KeyboardButton[] { "Complete registration" },
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
                "Could not find file",
                cancellationToken: cancellationToken);
        }
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
}
