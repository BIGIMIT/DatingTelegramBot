namespace DatingTelegramBot.Models;

public class UserView
{
    public int ViewerId { get; set; } // Id пользователя, который просматривает
    public User Viewer { get; set; }
    public int ViewedId { get; set; } // Id пользователя, который был просмотрен
    public User Viewed { get; set; }
    public bool? Like { get; set; }
}
