using DatingTelegramBot.Models;
using DatingTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DatingTelegramBot.Handlers.Searching;

public class SearchingMessageHandler : MessageHandler
{
    private readonly new IDbContextFactory<ApplicationDbContext> _contextFactory;
    public SearchingMessageHandler(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override string? Name { get; } = "SearchingMessageHandler";

    public override async Task HandleAsync(Models.User? user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (!CanHandle(user, update) || update.Message == null || update.Message.Text == null || _nextHandler == null || user == null)
        {
            await base.HandleAsync(user, botClient, update, cancellationToken);
            return;
        }
        using var context = _contextFactory.CreateDbContext();

        long chatId = update.Message.Chat.Id;

        string yesSend = PhraseDictionary.GetPhrase(user.Language, Phrases.Yes_Send);
        string rewrite = PhraseDictionary.GetPhrase(user.Language, Phrases.Rewrite);
        string back = PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching);

        if (update.Message.Text == yesSend)
        {
            await SendMessage(botClient, update, user, cancellationToken);
        }
        else if (update.Message.Text == rewrite)
        {
            await RewriteMessage(botClient, update, user, cancellationToken);
        }
        else if (update.Message.Text == back)
        {
            await HandleBackToSearching(botClient, update, user, cancellationToken);
        }
        else
        {
            await SaveMessage(user, botClient, update, cancellationToken);
        }


    }


    private async Task SaveMessage(Models.User user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update == null || update.Message == null || update.Message.Text == null) return;
        if (user == null || user.Gender == null) return;

        using var context = _contextFactory.CreateDbContext();
        UserView? userView = await GetNextUserView(user.Id, user.Gender);

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Yes_Send), PhraseDictionary.GetPhrase(user.Language, Phrases.Rewrite) },
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching) },
        })
        {
            ResizeKeyboard = true,
        };

        if (Program.UsersMessage.ContainsKey(user.Id))
        {
            Program.UsersMessage[user.Id] = update.Message.Text;
        }
        else
        {
            Program.UsersMessage.Add(user.Id, update.Message.Text);
        }
        
        await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Are_you_sure_you_want_to_send_a_message),
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
    }

    private async Task SendMessage(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();

        if (user == null || user.Gender == null) return;


        UserView? userView = await GetNextUserView(user.Id, user.Gender);

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching) },
            })
            {
                ResizeKeyboard = true,
            
            };

        if (userView != null && Program.UsersMessage != null && Program.UsersMessage.ContainsKey(user.Id))
        {
            userView.Message = Program.UsersMessage[user.Id];
            userView.Like = true;
            user.CurrentHandler = "SearchingStartHandler";


            Program.UsersMessage.Remove(user.Id);

            await botClient.SendTextMessageAsync(
                user.ChatId,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Message_saved_successfully),
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);

            context.UserViews.Update(userView);
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                user.ChatId,
                PhraseDictionary.GetPhrase(user.Language, Phrases.Message_not_found_please_try_again),
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

    }
    private async Task RewriteMessage(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        if (user == null || user.Gender == null) return;

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { PhraseDictionary.GetPhrase(user.Language, Phrases.Back_to_searching) },
        })
        {
            ResizeKeyboard = true
        };
        Program.UsersMessage?.Remove(user.Id);
        await botClient.SendTextMessageAsync(
          user.ChatId,
          PhraseDictionary.GetPhrase(user.Language, Phrases.Write_a_new_message),
          replyMarkup: replyKeyboardMarkup,
          cancellationToken: cancellationToken);

    }
    private async Task HandleBackToSearching(ITelegramBotClient botClient, Update update, Models.User? user, CancellationToken cancellationToken)
    {
        if (user == null) return;
        user.CurrentHandler = "SearchingStartHandler";
        user.Direction = false;
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        await base.HandleAsync(user, botClient, update, cancellationToken);
    }
    public async Task<Models.UserView?> GetNextUserView(int userId, string preferGender)
    {
        using var context = _contextFactory.CreateDbContext();


        var nextUser = await context.Users
            .Include(u => u.Photos)
            .Where(u => !context.UserViews.Any(uv => uv.ViewerId == userId && uv.ViewedId == u.Id && uv.Like != null) && u.Id != userId)
            .FirstOrDefaultAsync();

        if (nextUser == null)
        {
            return null;
        }

        var userView = await context.UserViews
            .Where(uv => uv.ViewerId == userId && uv.ViewedId == nextUser.Id)
            .FirstOrDefaultAsync();


        return userView;
    }

}