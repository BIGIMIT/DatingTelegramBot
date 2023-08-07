using Microsoft.EntityFrameworkCore;

namespace DatingTelegramBot.Models;


[Index(nameof(Name), IsUnique = true)]
public class Photo
{
    public int Id { get; set; }
    public string? Name { get; set; }   
    public string? FileId { get; set; }
    public string? Path { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public User User { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public int UserId { get; set; }
}
