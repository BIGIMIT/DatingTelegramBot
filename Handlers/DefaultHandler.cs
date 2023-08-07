using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DatingTelegramBot.Handlers;
public class DefaultHandler : MessageHandler
{
    public override string? Name { get; } = "DefaultHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        long chatId = update.Message.Chat.Id;

        // Send a default message if no other handlers could process the request
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Sorry, I couldn't process your request. Please try again later.",
            cancellationToken: cancellationToken);

    }
}
