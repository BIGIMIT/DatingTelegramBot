using Microsoft.EntityFrameworkCore;

namespace DatingTelegramBot.Models;

[Index(nameof(ChatId), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public int Age { get; set; }
    public string? Description { get; set; }
    public string? Gender { get; set; }
    public string? PreferGender { get; set; }
    public bool TurnOff { get; set; }
    public string? CurrentHandler { get; set; }
    public List<Photo>? Photos { get; set; }
    
    // Пользователи, которые этот пользователь просмотрел
    public List<UserView>? ViewedUsers { get; set; }
    // Пользователи, которые просмотрели этого пользователя
    public List<UserView>? ViewerUsers { get; set; }
    public List<UserMessage>? SentMessages { get; set; }
    public List<UserMessage>? ReceivedMessages { get; set; }
}

