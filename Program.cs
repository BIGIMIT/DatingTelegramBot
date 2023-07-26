using DatingTelegramBot.Handlers;
using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Program
{
    private static async Task Main(string[] args)
    {
        var configurationService = new ConfigurationService();
        var configuration = configurationService.GetConfiguration();
        var botClient = new TelegramBotClient(configuration["BotConfig:Token"]);

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Регистрируем наши сервисы
                DIConfig.ConfigureServices(services, configuration);
            })
            .Build();

        // Создаем обработчики для различных типов сообщений
        var startHandler = host.Services.GetRequiredService<StartHandler>();
        var defaultHandler = host.Services.GetRequiredService<DefaultHandler>();


        startHandler.SetNextHandler(defaultHandler);


        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token

        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = dbContextFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
            await startHandler.HandleAsync(user, botClient, update, cancellationToken);
        }


        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}