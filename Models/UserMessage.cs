namespace DatingTelegramBot.Models;

public class UserMessage
{
    public int SenderId { get; set; } // Id пользователя, который просматривает
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public User Sender { get; set; }
    public int ReceiverId { get; set; } // Id пользователя, который был просмотрен
    public User Receiver { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string? MessageText { get; set; }
}
