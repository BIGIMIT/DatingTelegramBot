using Microsoft.Extensions.DependencyInjection;
using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using DatingTelegramBot.Handlers;
using Microsoft.Extensions.Configuration;

namespace DatingTelegramBot.Services;
public static class DIConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем фабрику контекста базы данных
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Регистрируем обработчики
        services.AddTransient<StartHandler>();
        services.AddTransient<DefaultHandler>();
    }
}

