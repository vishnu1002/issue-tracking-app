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
    // Notifications removed

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

        // Notifications removed

        base.OnModelCreating(modelBuilder);
    }
}
