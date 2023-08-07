using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Account;

public class CreateAccountHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CreateAccountHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "CreateAccountHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || user == null || update.Message == null || update.Message.Text == null || _nextHandler == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();


        long chatId = update.Message.Chat.Id;

        user.CurrentHandler = _nextHandler.Name;
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Create An Account",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Please enter your name",
            cancellationToken: cancellationToken);


    }
}