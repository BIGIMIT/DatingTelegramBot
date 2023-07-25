using Microsoft.EntityFrameworkCore;

namespace DatingTelegramBot.Models;


[Index(nameof(Name), IsUnique = true)]
public class Photo
{
    public int Id { get; set; }
    public string? Name { get; set; }   
    public string? Source { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}
