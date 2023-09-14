using DatingTelegramBot.Handlers;
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
    public static Dictionary<int, string> UsersMessage = new();
    public static string[] Languages = { "Русский", "Українська", "English" }; 
    public static DateTime lastActivityTime = DateTime.UtcNow;
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
        var languageHandle = host.Services.GetRequiredService<LanguageHandle>();
        var agreeHandler = host.Services.GetRequiredService<AgreeHandler>();
        var accountUsernameHandler = host.Services.GetRequiredService<AccountUsernameHandler>();
        var createAccountHandler = host.Services.GetRequiredService<CreateAccountHandler>();
        var accountNameHandler = host.Services.GetRequiredService<AccountNameHandler>();
        var accountAgeHandler = host.Services.GetRequiredService<AccountAgeHandler>();
        var accountGenderHandler = host.Services.GetRequiredService<AccountGenderHandler>();
        var accountPreferredGenderHandler = host.Services.GetRequiredService<AccountPreferredGenderHandler>();
        var accountDescriptionHandler = host.Services.GetRequiredService<AccountDescriptionHandler>();
        var accountPhotoHandler = host.Services.GetRequiredService<AccountPhotoHandler>();
        var accountViewOrComplete = host.Services.GetRequiredService<AccountViewOrComplete>();
        var sendUserProfileHandler = host.Services.GetRequiredService<SendUserProfileHandler>();
        var searchingStartHandler = host.Services.GetRequiredService<SearchingStartHandler>();
        var searchingSettingsHandler = host.Services.GetRequiredService<SearchingSettingsHandler>();
        var searchingProfileHandle = host.Services.GetRequiredService<SearchingProfileHandle>();
        var changeAccountHandler = host.Services.GetRequiredService<ChangeAccountHandler>();
        var resumeSearchingHandler = host.Services.GetRequiredService<ResumeSearchingHandler>();
        var searchingMatches = host.Services.GetRequiredService<SearchingMatches>();
        var searchingMessageHandler = host.Services.GetRequiredService<SearchingMessageHandler>();
        var defaultHandler = host.Services.GetRequiredService<DefaultHandler>();


        startHandler.SetNextHandler(languageHandle);

        languageHandle.SetPreviousHandler(startHandler);
        languageHandle.SetNextHandler(agreeHandler);
        
        agreeHandler.SetPreviousHandler(languageHandle);
        agreeHandler.SetNextHandler(accountUsernameHandler);

        accountUsernameHandler.SetPreviousHandler(agreeHandler);
        accountUsernameHandler.SetNextHandler(createAccountHandler);

        createAccountHandler.SetPreviousHandler(accountUsernameHandler);
        createAccountHandler.SetNextHandler(accountNameHandler);

        accountNameHandler.SetPreviousHandler(createAccountHandler);
        accountNameHandler.SetNextHandler(accountAgeHandler);

        accountAgeHandler.SetPreviousHandler(accountNameHandler);
        accountAgeHandler.SetNextHandler(accountGenderHandler);

        accountGenderHandler.SetPreviousHandler(accountAgeHandler);
        accountGenderHandler.SetNextHandler(accountPreferredGenderHandler);

        accountPreferredGenderHandler.SetPreviousHandler(accountGenderHandler);
        accountPreferredGenderHandler.SetNextHandler(accountDescriptionHandler);

        accountDescriptionHandler.SetPreviousHandler(accountPreferredGenderHandler);
        accountDescriptionHandler.SetNextHandler(accountPhotoHandler);

        accountPhotoHandler.SetPreviousHandler(accountDescriptionHandler);
        accountPhotoHandler.SetNextHandler(accountViewOrComplete);

        accountViewOrComplete.SetPreviousHandler(accountPhotoHandler);
        accountViewOrComplete.SetNextHandler(sendUserProfileHandler);

        sendUserProfileHandler.SetPreviousHandler(accountViewOrComplete);
        sendUserProfileHandler.SetNextHandler(searchingStartHandler);

        searchingStartHandler.SetPreviousHandler(sendUserProfileHandler);
        searchingStartHandler.SetNextHandler(searchingSettingsHandler);

        searchingSettingsHandler.SetPreviousHandler(searchingStartHandler);
        searchingSettingsHandler.SetNextHandler(searchingProfileHandle);

        searchingProfileHandle.SetPreviousHandler(searchingSettingsHandler);
        searchingProfileHandle.SetNextHandler(searchingMatches);

        searchingMatches.SetPreviousHandler(searchingProfileHandle);
        searchingMatches.SetNextHandler(changeAccountHandler);

        changeAccountHandler.SetPreviousHandler(searchingMatches);
        changeAccountHandler.SetNextHandler(resumeSearchingHandler);

        resumeSearchingHandler.SetPreviousHandler(changeAccountHandler);
        resumeSearchingHandler.SetNextHandler(searchingMessageHandler);

        searchingMessageHandler.SetPreviousHandler(resumeSearchingHandler);
        searchingMessageHandler.SetNextHandler(defaultHandler);

        defaultHandler.SetPreviousHandler(searchingMessageHandler);



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
            if (message.Date <= lastActivityTime) return;
            if (message.Text == null && message.Photo == null)
                return;

            var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = dbContextFactory.CreateDbContext();
            var user = await context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);

            if (message.Text != null)
                Console.WriteLine($"Received a message in chat {chatId} - '{message.Text}' .");
            else if (message.Photo != null)
                Console.WriteLine($"Received a message in chat {chatId} - '{message.Photo.Last().FileId}' .");

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