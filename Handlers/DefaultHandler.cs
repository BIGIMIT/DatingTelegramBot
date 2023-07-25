using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;
public class DefaultHandler : MessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public DefaultHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "DefaultHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using (var context = _contextFactory.CreateDbContext())
        {

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            long chatId = update.Message.Chat.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Send a default message if no other handlers could process the request
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Sorry, I couldn't process your request. Please try again later.",
                cancellationToken: cancellationToken);
        }
    }
}
