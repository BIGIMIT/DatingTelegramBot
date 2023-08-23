﻿using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class AccountPreferredGenderHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountPreferredGenderHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "AccountPreferredGenderHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        
        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;

        if (text == "Woman" || text == "Man" || text == "Both")
        {
            using var context = _contextFactory.CreateDbContext();

            user.CurrentHandler = _nextHandler.Name;
            user.PreferGender = text;
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Write semething bout you",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
        }
    }
}