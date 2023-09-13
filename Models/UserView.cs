namespace DatingTelegramBot.Models;

public class UserView
{
    public int ViewerId { get; set; } // Id пользователя, который просматривает
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public User Viewer { get; set; }
    public int ViewedId { get; set; } // Id пользователя, который был просмотрен
    public User Viewed { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public bool? Like { get; set; }
    public string? Message { get; set; }
    public bool WasShown { get; set; }
}
