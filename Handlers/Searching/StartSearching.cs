using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Searching;

public class StartSearching : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StartSearching(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "StartSearching";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        long chatId = update.Message.Chat.Id;

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "StartSearching",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

    }
}