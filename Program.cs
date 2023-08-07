﻿using DatingTelegramBot.Handlers;
using DatingTelegramBot.Handlers.Account;
using DatingTelegramBot.Handlers.Searching;
using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DatingTelegramBot;

public class Program
{
    private static async Task Main(string[] args)
    {
        var configurationService = new ConfigurationService();
        var configuration = ConfigurationService.GetConfiguration();
#pragma warning disable CS8604 // Possible null reference argument.
        var botClient = new TelegramBotClient(configuration["BotConfig:Token"]);
#pragma warning restore CS8604 // Possible null reference argument.

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Регистрируем наши сервисы
                DIConfig.ConfigureServices(services, configuration);
            })
            .Build();

        // Создаем обработчики для различных типов сообщений
        var startHandler = host.Services.GetRequiredService<StartHandler>();
        var agreeHandler = host.Services.GetRequiredService<AgreeHandler>();
        var createAccountHandler = host.Services.GetRequiredService<CreateAccountHandler>();
        var accountNameHandler = host.Services.GetRequiredService<AccountNameHandler>();
        var accountAgeHandler = host.Services.GetRequiredService<AccountAgeHandler>();
        var accountGenderHandler = host.Services.GetRequiredService<AccountGenderHandler>();
        var accountPreferredGenderHandler = host.Services.GetRequiredService<AccountPreferredGenderHandler>();
        var accountDescriptionHandler = host.Services.GetRequiredService<AccountDescriptionHandler>();
        var accountPhotoHandler = host.Services.GetRequiredService<AccountPhotoHandler>();
        var accountViewOrComplete = host.Services.GetRequiredService<AccountViewOrComplete>();
        var sendUserProfileHandler = host.Services.GetRequiredService<SendUserProfileHandler>();
        var startSearching = host.Services.GetRequiredService<SearchingStartHandler>();
        var defaultHandler = host.Services.GetRequiredService<DefaultHandler>();


        startHandler.SetNextHandler(agreeHandler);
        agreeHandler.SetNextHandler(createAccountHandler);
        createAccountHandler.SetNextHandler(accountNameHandler);
        accountNameHandler.SetNextHandler(accountAgeHandler);
        accountAgeHandler.SetNextHandler(accountGenderHandler);
        accountGenderHandler.SetNextHandler(accountPreferredGenderHandler);
        accountPreferredGenderHandler.SetNextHandler(accountDescriptionHandler);
        accountDescriptionHandler.SetNextHandler(accountPhotoHandler);
        accountPhotoHandler.SetNextHandler(accountViewOrComplete);
        accountViewOrComplete.SetNextHandler(sendUserProfileHandler);
        sendUserProfileHandler.SetNextHandler(startSearching);
        startSearching.SetNextHandler(defaultHandler);


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
            var chatId = message.Chat.Id;
            // Only process text messages
            if (message.Text == null && message.Photo == null)
                return;

            var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = dbContextFactory.CreateDbContext();
            var user = await context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);

            if (message.Text != null)
                Console.WriteLine($"Received a '{message.Text}' message in chat {chatId}.");
            else if (message.Photo != null)
                Console.WriteLine($"Received a '{message.Photo.Last().FileId}' message in chat {chatId}.");

            await startHandler.HandleAsync(user, botClient, update, cancellationToken);
            return;


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