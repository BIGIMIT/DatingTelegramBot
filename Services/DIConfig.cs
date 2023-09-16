using Microsoft.Extensions.DependencyInjection;
using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using DatingTelegramBot.Handlers;
using Microsoft.Extensions.Configuration;
using DatingTelegramBot.Handlers.Account;
using DatingTelegramBot.Handlers.Searching;

namespace DatingTelegramBot.Services;
public static class DIConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем фабрику контекста базы данных
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));

        // Регистрируем обработчики
        services.AddTransient<StartHandler>();
        services.AddTransient<LanguageHandle>();
        services.AddTransient<AgreeHandler>();
        services.AddTransient<AccountUsernameHandler>();
        services.AddTransient<CreateAccountHandler>();
        services.AddTransient<AccountNameHandler>();
        services.AddTransient<AccountAgeHandler>();
        services.AddTransient<AccountGenderHandler>();
        services.AddTransient<AccountPreferredGenderHandler>();
        services.AddTransient<AccountDescriptionHandler>();
        services.AddTransient<AccountPhotoHandler>();
        services.AddTransient<AccountViewOrComplete>();
        services.AddTransient<SendUserProfileHandler>();
        services.AddTransient<SearchingStartHandler>();
        services.AddTransient<SearchingSettingsHandler>();
        services.AddTransient<SearchingProfileHandle>();
        services.AddTransient<ChangeAccountHandler>();
        services.AddTransient<ResumeSearchingHandler>();
        services.AddTransient<SearchingMatches>();
        services.AddTransient<SearchingMessageHandler>();
        services.AddTransient<DefaultHandler>();
    }
}

