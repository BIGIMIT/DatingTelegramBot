using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;

public class LanguageHandle : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public LanguageHandle(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "LanguageHandle";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) ||user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;
        if (Program.Languages.Contains(update.Message.Text))
        {
            user.CurrentHandler = _nextHandler.Name;
            user.Language = update.Message.Text;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        else
        {

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose language below",
                cancellationToken: cancellationToken);
        }

    }
}