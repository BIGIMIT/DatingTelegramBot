﻿using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers;
public class StartHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StartHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "StartHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update))
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();
        

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            long chatId = update.Message.Chat.Id;
            
            if (user != null)
            {
                user.CurrentHandler = _nextHandler.Name;
                context.Users.Update(user);
                await context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var newUser = new Models.User
                {
                    ChatId = chatId,
                    CurrentHandler = _nextHandler.Name,
                    TurnOff = true,
                };
                await context.Users.AddAsync(newUser, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "Continue" },
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Welcome",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        

    }
}
