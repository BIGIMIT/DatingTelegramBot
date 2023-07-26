using Microsoft.Extensions.Configuration;

namespace DatingTelegramBot.Services;

public class ConfigurationService
{
    public IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Secrets.json", optional: true)
            .Build();
    }
}
