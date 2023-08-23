using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers.Searching;

public class ResumeSearchingHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ResumeSearchingHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "ResumeSearchingHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text != "Resume searching" )
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;

        user.CurrentHandler = "SearchingStartHandler";
        user.Direction = false;
        user.TurnOff = false;
        user.Name = update.Message.Text;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await base.HandleAsync(user, botClient, update, cancellationToken);
    }
}