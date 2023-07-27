using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DatingTelegramBot.Handlers;

public class AccountConfirmPhotoHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountConfirmPhotoHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountConfirmPhotoHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

        if (!CanHandle(user, update) || update.Message.Photo == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        long chatId = update.Message.Chat.Id;

        using var context = _contextFactory.CreateDbContext();

        if (update?.Message?.Text == "Yes" || update?.Message?.Text == "Change")
        {
            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Send next photo",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            user.CurrentHandler = "AccountPhotoHandler";
            context.Users.Update(user);
        }
        else if (update?.Message?.Text == "No" || update?.Message?.Text == "Finish registration")
        {
            user.CurrentHandler = _nextHandler.Name;
            context.Users.Update(user);

            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Ok, let's see your profile",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);

            StringBuilder profile = new StringBuilder();

            profile.Append($"Name: {user.Name} \n");
            profile.Append($"Age: {user.Age} \n");
            profile.Append($"Description: {user.Description} \n");

            await using Stream stream = System.IO.File.OpenRead("../hamlet.pdf");

        }
        else
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }

        await context.SaveChangesAsync(cancellationToken);

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}