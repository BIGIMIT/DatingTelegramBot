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

namespace DatingTelegramBot.Handlers.Account;

public class ChangeAccountHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ChangeAccountHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "ChangeAccountHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();


        long chatId = update.Message.Chat.Id;

        user.CurrentHandler = "AccountNameHandler";
        user.Direction = true;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Change your profile",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Please enter your name",
            cancellationToken: cancellationToken);


    }
}