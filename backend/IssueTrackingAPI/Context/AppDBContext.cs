using IssueTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net.Sockets;

namespace IssueTrackingAPI.Context;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions options) : base(options) { }

    // Tables
    public DbSet<UserModel> Users_Table { get; set; }
    public DbSet<TicketModel> Tickets_Table { get; set; }
    public DbSet<AttachmentModel> Attachments_Table { get; set; }
    public DbSet<NotificationModel> Notifications_Table { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User-Ticket relationships
        modelBuilder.Entity<TicketModel>()
            .HasOne(t => t.CreatedByUser)           // Each ticket must have 1 user
            .WithMany(u => u.CreatedTickets)        // A user can have many tickets
            .HasForeignKey(t => t.CreatedByUserId)  // CreatedByUserId (FK in Ticket Table)
            .OnDelete(DeleteBehavior.Restrict);     // Dont auto delete tickets if user is deleted

        modelBuilder.Entity<TicketModel>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket-Attachment relationship
        modelBuilder.Entity<AttachmentModel>()
            .HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId);

        // User-Notification relationship
        modelBuilder.Entity<NotificationModel>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ticket-Notification relationship
        modelBuilder.Entity<NotificationModel>()
            .HasOne(n => n.Ticket)
            .WithMany()
            .HasForeignKey(n => n.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
