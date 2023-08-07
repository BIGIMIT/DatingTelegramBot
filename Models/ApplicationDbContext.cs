using Microsoft.EntityFrameworkCore;

namespace DatingTelegramBot.Models;
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<UserView> UserViews { get; set; }
    public DbSet<UserMessage> UserMessages { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserView>()
            .HasKey(uv => new { uv.ViewerId, uv.ViewedId });

        modelBuilder.Entity<UserView>()
            .HasOne(uv => uv.Viewer)
            .WithMany(u => u.ViewedUsers)
            .HasForeignKey(uv => uv.ViewerId)
            .OnDelete(DeleteBehavior.Restrict); ;

        modelBuilder.Entity<UserView>()
            .HasOne(uv => uv.Viewed)
            .WithMany(u => u.ViewerUsers)
            .HasForeignKey(uv => uv.ViewedId);


        modelBuilder.Entity<UserMessage>()
            .HasKey(um => new { um.SenderId, um.ReceiverId });

        modelBuilder.Entity<UserMessage>()
            .HasOne(um => um.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(um => um.SenderId)
            .OnDelete(DeleteBehavior.Restrict); ;

        modelBuilder.Entity<UserMessage>()
            .HasOne(um => um.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(um => um.ReceiverId);
    }
}